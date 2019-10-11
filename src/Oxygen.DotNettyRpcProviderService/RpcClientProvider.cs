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
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
namespace Oxygen.DotNettyRpcProviderService
{
    public delegate void ReceiveHander(RpcGlobalMessageBase<object> message);

    /// <summary>
    /// 客户端消息服务类
    /// </summary>
    public class RpcClientProvider : IRpcClientProvider
    {
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        private readonly IFlowControlCenter _flowControlCenter;
        private readonly CustomerInfo _customerInfo;
        public static readonly ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>> TaskHookInfos =
            new ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>>();
        #region dotnetty相关
        static Bootstrap _bootstrap;
        static readonly ConcurrentDictionary<string, IChannel> Channels = new ConcurrentDictionary<string, IChannel>();
        #endregion

        public RpcClientProvider(IOxygenLogger logger, ISerialize serialize, IFlowControlCenter flowControlCenter, CustomerInfo customerInfo)
        {
            _logger = logger;
            _serialize = serialize;
            _flowControlCenter = flowControlCenter;
            _bootstrap = CreateBootStrap();
            _customerInfo = customerInfo;
        }
        /// <summary>
        /// 创建Bootstrap
        /// </summary>
        /// <returns></returns>
        Bootstrap CreateBootStrap()
        {
            IEventLoopGroup group;
            var bootstrap = new Bootstrap();
            group = new EventLoopGroup();
            bootstrap.Channel<TcpChannel>();
            bootstrap
                        .Group(group)
                        .Option(ChannelOption.TcpNodelay, true)
                        .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                        .Option(ChannelOption.ConnectTimeout, new TimeSpan(0, 0, 5))
                        .Handler(new ActionChannelInitializer<IChannel>(ch =>
                        {
                            var pipeline = ch.Pipeline;
                            pipeline.AddLast(new LengthFieldPrepender(4));
                            pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                            pipeline.AddLast(new MessageDecoder<RpcGlobalMessageBase<object>>(_serialize));
                            pipeline.AddLast(new MessageEncoder<object>(_serialize));
                            pipeline.AddLast(new RpcClientHandler(_logger, ReceiveMessage));
                        }));
            return bootstrap;
        }

        /// <summary>
        /// 创建客户端实例
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<string> CreateClient(IPEndPoint endPoint)
        {
            return await CreateChannel(endPoint);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<T> SendMessage<T>(string channelKey, IPEndPoint endPoint, string flowControlCfgKey, ServiceConfigureInfo configure, string key, string path, object message) where T : class
        {
            T result = default;
            if (Channels.TryGetValue(channelKey, out var _channel))
            {
                try
                {
                    result = await _flowControlCenter.ExcuteAsync(key, endPoint, flowControlCfgKey, configure, async () =>
                    {
                        var taskId = Guid.NewGuid();
                        var sendMessage = new RpcGlobalMessageBase<object>
                        {
                            CustomerIp = _customerInfo.Ip,
                            TaskId = taskId,
                            Path = path,
                            Message = message is string ? _serialize.Deserializes<object>(_serialize.SerializesJsonString((string)message)) : message
                        };
                        sendMessage.Sign(GlobalCommon.SHA256Encrypt(taskId + OxygenSetting.SignKey));
                        var resultTask = RegisterResultCallbackAsync(taskId);
                        await _channel.WriteAndFlushAsync(sendMessage);
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
        async Task<string> CreateChannel(IPEndPoint endpoint)
        {
            try
            {
                var channelKey = $"{endpoint.Address}{endpoint.Port}";
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
        void ReceiveMessage(RpcGlobalMessageBase<object> message)
        {
            if (message != null)
            {
                switch (message.code)
                {
                    case HttpStatusCode.OK:
                        var task = GetHook(message.TaskId);
                        task?.TrySetResult(_serialize.Serializes(message.Message));
                        break;
                    case HttpStatusCode.NotFound:
                        _logger.LogError("RPC调用失败,未找到对应的消费者应用程序!");
                        task = GetHook(message.TaskId);
                        task?.TrySetResult(null);
                        break;
                    case HttpStatusCode.Unauthorized:
                        _logger.LogError("RPC调用失败,数字签名验签不通过!");
                        task = GetHook(message.TaskId);
                        task?.TrySetResult(null);
                        break;
                }
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
