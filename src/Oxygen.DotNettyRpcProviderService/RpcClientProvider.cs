using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Oxygen.DotNettyRpcProviderService
{
    public delegate void ReceiveHander(byte[] input);

    /// <summary>
    /// 客户端消息服务类
    /// </summary>
    public class RpcClientProvider : IRpcClientProvider
    {
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        public static readonly ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>> TaskHookInfos =
            new ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>>();
        #region dotnetty相关
        static Bootstrap _bootstrap;
        static readonly ConcurrentDictionary<EndPoint, IChannel> Channels = new ConcurrentDictionary<EndPoint, IChannel>();
        #endregion

        public RpcClientProvider(IOxygenLogger logger, ISerialize serialize)
        {
            _logger = logger;
            _serialize = serialize;
            _bootstrap = CreateBootStrap();
        }

        Bootstrap CreateBootStrap()
        {
            return new Bootstrap()
                .Group(new EventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    pipeline.AddLast(new RpcClientHandler(_logger, ReceiveMessage));
                }));
        }

        /// <summary>
        /// 创建客户端连接
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public async Task CreateClient(IPEndPoint endPoint)
        {
            await CreateChannel(endPoint);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endPoint"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<T> SendMessage<T>(EndPoint endPoint, string path, object message)
        {
            T result = default(T);
            if (Channels.TryGetValue(endPoint, out var _channel))
            {
                var taskId = Guid.NewGuid();
                var sendMessage = new RpcGlobalMessageBase<object>
                {
                    TaskId = taskId,
                    Path = path,
                    Message = message is string ? _serialize.Deserializes<object>(_serialize.SerializesJsonString((string)message)) : message
                };
                var resultTask = RegisterResultCallbackAsync(taskId);
                try
                {
                    var buffer = Unpooled.WrappedBuffer(_serialize.Serializes(sendMessage));
                    await _channel.WriteAndFlushAsync(buffer);
                    var resultBt = await resultTask;
                    if (resultBt != null && resultBt.Any())
                    {
                        result = _serialize.Deserializes<T>(resultBt);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"客户端发送消息异常:{e.Message}");
                }
            }
            return result;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<object> SendMessage(EndPoint endPoint, string path, object message)
        {
            object result = default(object);
            if (Channels.TryGetValue(endPoint, out var _channel))
            {
                var taskId = Guid.NewGuid();
                var sendMessage = new RpcGlobalMessageBase<object>
                {
                    TaskId = taskId,
                    Path = path,
                    Message = message
                };
                var resultTask = RegisterResultCallbackAsync(taskId);
                try
                {
                    var buffer = Unpooled.WrappedBuffer(_serialize.Serializes(sendMessage));
                    await _channel.WriteAndFlushAsync(buffer);
                    var resultBt = await resultTask;
                    if (resultBt != null && resultBt.Any())
                    {
                        result = _serialize.Deserializes<object>(resultBt);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"客户端发送消息异常:{e.Message}");
                }
            }
            return result;
        }

        #region 私有方法
        /// <summary>
        /// 创建通道
        /// </summary>
        /// <returns></returns>
        async Task CreateChannel(IPEndPoint endPoint)
        {
            if (Channels.TryGetValue(endPoint, out var channel))
            {
                if (!channel.Active)
                {
                    await CloseChannel(channel);
                    Channels.TryRemove(endPoint, out channel);
                }
            }
            else
            {
                var newChannel = await _bootstrap.ConnectAsync("127.0.0.1", endPoint.Port);
                if (newChannel.Active)
                {
                    Channels.TryAdd(endPoint, newChannel);
                }
                else
                {
                    await CloseChannel(channel);
                    Channels.TryRemove(endPoint, out channel);
                }
            }
        }
        /// <summary>
        /// 消息回调处理
        /// </summary>
        /// <param name="input"></param>
        void ReceiveMessage(byte[] input)
        {
            if (input != null || input.Any())
            {
                var message = _serialize.Deserializes<RpcGlobalMessageBase<object>>(input);
                var task = GetHook(message.TaskId);
                task?.TrySetResult(_serialize.Serializes(message.Message));
            }
        }
        /// <summary>
        /// 消息钩子
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        async Task<byte[]> RegisterResultCallbackAsync(Guid id)
        {
            var task = new TaskCompletionSource<byte[]>();
            SetHook(id, task);
            try
            {
                var result = await task.Task;
                return result;
            }
            finally
            {
                RemoveHook(id);
                task.TrySetCanceled();
            }
        }

        /// <summary>
        /// 删除通道消息
        /// </summary>
        /// <returns></returns>
        async Task CloseChannel(IChannel channel)
        {
            await channel.CloseAsync();
            _bootstrap.RemoteAddress(channel.RemoteAddress);
        }


        /// <summary>
        /// 获取钩子
        /// </summary>
        /// <returns></returns>
        TaskCompletionSource<byte[]> GetHook(Guid id)
        {
            TaskHookInfos.TryGetValue(id, out var value);
            return value;
        }
        /// <summary>
        /// 删除钩子
        /// </summary>
        /// <param name="id"></param>
        void RemoveHook(Guid id)
        {
            TaskHookInfos.TryRemove(id, out TaskCompletionSource<byte[]> task);
        }

        /// <summary>
        /// 设置钩子
        /// </summary>
        void SetHook(Guid id, TaskCompletionSource<byte[]> message)
        {
            TaskHookInfos.TryAdd(id, message);
        }
        #endregion
    }
}
