using Microsoft.AspNetCore.Http;
using Oxygen.CommonTool;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Oxygen.KestrelRpcProviderService
{
    /// <summary>
    /// 协议消息体构造器
    /// </summary>
    public class ProtocolMessageBuilder
    {
        private readonly ISerialize _serialize;
        public ProtocolMessageBuilder(ISerialize serialize)
        {
            _serialize = serialize;
        }
        /// <summary>
        /// 构造客户端请求消息体
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="pathName"></param>
        /// <param name="input"></param>
        /// <returns></returns>

        public HttpRequestMessage GetClientSendMessage(Guid taskId, string serverName, string pathName, object input, Dictionary<string, string> traceHeaders = null)
        {
            var sendMessage = new RpcGlobalMessageBase<object>
            {
                TaskId = taskId,
                Path = pathName,
                Message = input is string ? _serialize.Deserializes<object>(_serialize.SerializesJsonString((string)input)) : input
            };
            switch (OxygenSetting.ProtocolType)
            {
                default:
                case EnumProtocolType.HTTP2:
                    byte[] json = _serialize.Serializes(sendMessage);
                    var request = new HttpRequestMessage(HttpMethod.Post,$"http://{serverName}:{OxygenSetting.ServerPort}/{pathName}") { Version = new Version(2, 0) };
                    request.Content = new ByteArrayContent(json);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-msgpack");
                    request.Content.Headers.ContentLength = json.Length;
                    TraceHeaderHelper.BuildTraceHeader(request.Content.Headers, traceHeaders);
                    return request;
            }
        }
        public (RpcGlobalMessageBase<object> messageBase, Dictionary<string, string> traceHeaders) GetReceiveMessage(HttpContext message)
        {
            using (var buffer = new MemoryStream())
            {
                message.Request.Body.CopyTo(buffer);
                byte[] bytes = buffer.ToArray();
                buffer.Write(bytes, 0, Convert.ToInt32(buffer.Length));
                var result = _serialize.Deserializes<RpcGlobalMessageBase<object>>(bytes);
                Dictionary<string, string> traceHeaders = default;
                if (message.Request.Headers.Any())
                {
                    traceHeaders = TraceHeaderHelper.GetTraceHeaders(message.Request.Headers);
                }
                return (result, traceHeaders);
            }
        }
    }
}
