using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.KestrelRpcProviderService
{
    public class RpcClientProvider : IRpcClientProvider
    {
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        private readonly ProtocolMessageBuilder protocolMessageBuilder;
        private ConcurrentDictionary<string, HttpClient> DicClient =new ConcurrentDictionary<string, HttpClient>();
        public RpcClientProvider(IOxygenLogger logger, ISerialize serialize)
        {
            _logger = logger;
            _serialize = serialize;
            protocolMessageBuilder = new ProtocolMessageBuilder(serialize);
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);//非https环境下设置上下文支持http2连接
        }
        public async Task<bool> CreateClient(string serverName)
        {
            DicClient.TryAdd(serverName, new HttpClient());
            return await Task.FromResult(true);
        }

        public async Task<T> SendMessage<T>(string serverName, string pathName, object input, Dictionary<string, string> traceHeaders = null) where T : class
        {
            return await SendMessage<T>(serverName, pathName, input, null, traceHeaders);
        }

        public async Task<object> SendMessage(string serverName, string pathName, object input, Type returnType, Dictionary<string, string> traceHeaders = null)
        {
            return await SendMessage<object>(serverName, pathName, input, returnType, traceHeaders);
        }
        private async Task<T> SendMessage<T>(string serverName, string pathName, object input, Type returnType, Dictionary<string, string> traceHeaders = null) where T : class
        {
            T result = default;
            if (DicClient.TryGetValue(serverName, out var _httpclient))
            {
                try
                {
                    var taskId = Guid.NewGuid();
                    var sendMessage = protocolMessageBuilder.GetClientSendMessage(taskId, serverName, pathName, input, traceHeaders);
                    var responseMessage = await _httpclient.SendAsync(sendMessage);
                    if (responseMessage != null)
                    {
                        var resultBt = ReceiveMessage(await responseMessage.Content.ReadAsByteArrayAsync());
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
        byte[] ReceiveMessage(byte[] resultBt)
        {
            var message= _serialize.Deserializes<RpcGlobalMessageBase<object>>(resultBt);
            if (message != null)
            {
                switch (message.code)
                {
                    case HttpStatusCode.OK:
                        return _serialize.Serializes(message.Message);
                    case HttpStatusCode.NotFound:
                        _logger.LogError("RPC调用失败,未找到对应的消费者应用程序!");
                        return default;
                    case HttpStatusCode.Unauthorized:
                        _logger.LogError("RPC调用失败,数字签名验签不通过!");
                        return default;
                }
            }
            return default;
        }
    }
}
