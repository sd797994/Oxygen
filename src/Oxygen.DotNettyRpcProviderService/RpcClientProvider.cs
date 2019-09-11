using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using Oxygen.IServerFlowControl;
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
        private readonly IFlowControlCenter _flowControlCenter;
        private readonly IGlobalCommon _globalCommon;
        private readonly CustomerIp _customerIp;
        public static readonly ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>> TaskHookInfos =
            new ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>>();
        #region dotnetty相关
        static Bootstrap _bootstrap;
        static readonly ConcurrentDictionary<string, IChannel> Channels = new ConcurrentDictionary<string, IChannel>();
        #endregion

        public RpcClientProvider(IOxygenLogger logger, ISerialize serialize, IFlowControlCenter flowControlCenter, IGlobalCommon globalCommon, CustomerIp customerIp)
        {
            _logger = logger;
            _serialize = serialize;
            _flowControlCenter = flowControlCenter;
            _bootstrap = CreateBootStrap();
            _customerIp = customerIp;
            _globalCommon = globalCommon;
        }
        /// <summary>
        /// 创建Bootstrap
        /// </summary>
        /// <returns></returns>
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
        /// 创建客户端实例
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<string> CreateClient(IPEndPoint endPoint, string serverName, string path)
        {
            return await CreateChannel(endPoint, serverName, path);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<T> SendMessage<T>(string channelKey, IPEndPoint endPoint, string flowControlCfgKey, object configureInfo, string key, string path, object message) where T : class
        {
            T result = default;
            if (Channels.TryGetValue(channelKey, out var _channel))
            {
                try
                {
                    result = await _flowControlCenter.ExcuteAsync(key, endPoint, flowControlCfgKey,(ServiceConfigureInfo)configureInfo, async () =>
                    {
                        var taskId = Guid.NewGuid();
                        var sendMessage = new RpcGlobalMessageBase<object>
                        {
                            CustomerIp = _customerIp.Ip,
                            TaskId = taskId,
                            Path = path,
                            Message = message is string ? _serialize.Deserializes<object>(_serialize.SerializesJsonString((string)message)) : message
                        };
                        var resultTask = RegisterResultCallbackAsync(taskId);
                        var buffer = Unpooled.WrappedBuffer(_globalCommon.RsaEncryp(_serialize.Serializes(sendMessage)));
                        await _channel.WriteAndFlushAsync(buffer);
                        var resultBt = await resultTask;
                        if (resultBt != null && resultBt.Any())
                        {
                            return _serialize.Deserializes<T>(resultBt);
                        }
                        return default;
                    });
                }
                catch (Exception e)
                {
                    //ignore异常，等待polly处理
                }
            }
            return result;
        }

        #region 私有方法
        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        async Task<string> CreateChannel(IPEndPoint endpoint, string serviceName, string path)
        {
            try
            {
                var channelKey = $"{endpoint.Address}{endpoint.Port}{serviceName}{path}";
                if (Channels.TryGetValue(channelKey, out var channel))
                {
                    if (!channel.Active)
                    {
                        await CloseChannel(channel);
                        Channels.TryRemove(channelKey, out channel);
                    }
                    else
                    {
                        return channelKey;
                    }
                }
                else
                {
                    var newChannel = await _bootstrap.ConnectAsync(endpoint);
                    if (newChannel.Active)
                    {
                        Channels.TryAdd(channelKey, newChannel);
                        return channelKey;
                    }
                    else
                    {
                        await CloseChannel(channel);
                        Channels.TryRemove(channelKey, out channel);
                    }
                }
            }
            catch(Exception)
            {
                return null;
            }
            return null;
        }
        /// <summary>
        /// 消息回调处理
        /// </summary>
        /// <param name="input"></param>
        void ReceiveMessage(byte[] input)
        {
            if (input != null || input.Any())
            {
                var message = _serialize.Deserializes<RpcGlobalMessageBase<object>>(_globalCommon.RsaDecrypt(input));
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
            TaskHookInfos.TryRemove(id, out _);
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
