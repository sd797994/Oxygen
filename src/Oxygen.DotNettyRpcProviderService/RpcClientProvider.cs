using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
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
        public static readonly ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>> TaskHookInfos =
            new ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>>();
        private readonly ProtocolMessageBuilder protocolMessageBuilder;
        #region dotnetty相关
        static Bootstrap _bootstrap;
        static readonly ConcurrentDictionary<string, IChannel> Channels = new ConcurrentDictionary<string, IChannel>();
        #endregion

        public RpcClientProvider(IOxygenLogger logger, ISerialize serialize)
        {
            _logger = logger;
            _serialize = serialize;
            _bootstrap = new BootstrapFactory(logger, serialize).CreateClientBootstrap(ReceiveMessage);
            protocolMessageBuilder = new ProtocolMessageBuilder(serialize);
        }

        /// <summary>
        /// 创建客户端实例
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> CreateClient(string serverName)
        {
            try
            {
                if (Channels.TryGetValue(serverName, out var channel))
                {
                    if (!channel.Active)
                    {
                        await CloseChannel(channel);
                        Channels.TryRemove(serverName, out channel);
                        return await CreateNewChannel(serverName);
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                   return await CreateNewChannel(serverName);
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        /// <summary>
        /// 创建新的通道
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        private async Task<bool> CreateNewChannel(string serverName)
        {
            var newChannel = await _bootstrap.ConnectAsync(serverName, OxygenSetting.ServerPort);
            //var newChannel = await _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), OxygenSetting.ServerPort));
            if (newChannel.Active)
            {
                Channels.TryAdd(serverName, newChannel);
                return true;
            }
            else
            {
                await CloseChannel(newChannel);
                Channels.TryRemove(serverName, out newChannel);
                return false;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<T> SendMessage<T>(string serverName, string pathName, object input, Dictionary<string, string> traceHeaders = null) where T : class
        {
            return await SendMessage<T>(serverName, pathName, input, null, traceHeaders);
        }
        public async Task<object> SendMessage(string serverName, string pathName, object input, Type returnType, Dictionary<string, string> traceHeaders = null)
        {
            return await SendMessage<object>(serverName, pathName, input, returnType, traceHeaders);
        }
        #region 私有方法
        /// <summary>
        /// 发送消息到远程服务器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverName"></param>
        /// <param name="pathName"></param>
        /// <param name="input"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        private async Task<T> SendMessage<T>(string serverName, string pathName, object input, Type returnType, Dictionary<string, string> traceHeaders = null) where T : class
        {
            T result = default;
            if (Channels.TryGetValue(serverName, out var _channel))
            {
                try
                {
                    var taskId = Guid.NewGuid();
                    var sendMessage = protocolMessageBuilder.GetClientSendMessage(taskId, serverName, pathName, input, traceHeaders);
                    var resultTask = RegisterResultCallbackAsync(taskId);
                    await _channel.WriteAndFlushAsync(sendMessage);
                    var resultBt = await resultTask;
                    if (resultBt != null && resultBt.Any())
                    {
                        if (returnType == null)
                            return _serialize.Deserializes<T>(resultBt);
                        else
                            return _serialize.Deserializes(returnType, resultBt) as T;
                    }
                    return default;
                }
                catch (Exception e)
                {
                    _logger.LogError($"调用异常：{e.Message},调用堆栈{e.StackTrace.ToString()}");
                }
            }
            return result;
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
