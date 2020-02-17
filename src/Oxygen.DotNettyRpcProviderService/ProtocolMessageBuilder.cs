using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using Oxygen.CommonTool;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.DotNettyRpcProviderService
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

        public object GetClientSendMessage(Guid taskId, string serverName, string pathName, object input, Dictionary<string, string> traceHeaders = null)
        {
            var sendMessage = new RpcGlobalMessageBase<object>
            {
                TaskId = taskId,
                Path = pathName,
                Message = input is string ? _serialize.Deserializes<object>(_serialize.SerializesJsonString((string)input)) : input
            };
            switch (OxygenSetting.ProtocolType)
            {
                case EnumProtocolType.HTTP11:
                    byte[] json = Encoding.UTF8.GetBytes(_serialize.SerializesJson(sendMessage));
                    var request = new DefaultFullHttpRequest(HttpVersion.Http11, HttpMethod.Post, $"http://{serverName}", Unpooled.WrappedBuffer(json), false);
                    HttpHeaders headers = request.Headers;
                    headers.Set(HttpHeaderNames.ContentType, AsciiString.Cached("application/json"));
                    headers.Set(HttpHeaderNames.ContentLength, AsciiString.Cached($"{json.Length}"));
                    TraceHeaderHelper.BuildTraceHeader(headers, traceHeaders);
                    return request;
                case EnumProtocolType.TCP:
                default:
                    return sendMessage;
            }
        }
        /// <summary>
        /// 构造服务端发送消息体
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public object GetServerSendMessage(RpcGlobalMessageBase<object> message, Dictionary<string, string> traceHeaders = null)
        {
            switch (OxygenSetting.ProtocolType)
            {
                case EnumProtocolType.HTTP11:
                    byte[] json = Encoding.UTF8.GetBytes(_serialize.SerializesJson(message));
                    var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, Unpooled.WrappedBuffer(json), false);
                    HttpHeaders headers = response.Headers;
                    headers.Set(HttpHeaderNames.ContentType, AsciiString.Cached("application/json"));
                    headers.Set(HttpHeaderNames.Server, "dotnetty");
                    headers.Set(HttpHeaderNames.Date, AsciiString.Cached($"{DateTime.UtcNow.DayOfWeek}, {DateTime.UtcNow:dd MMM yyyy HH:mm:ss z}"));
                    headers.Set(HttpHeaderNames.ContentLength, AsciiString.Cached($"{json.Length}"));
                    TraceHeaderHelper.BuildTraceHeader(headers, traceHeaders);
                    return response;
                case EnumProtocolType.TCP:
                default:
                    return message;
            }
        }
        /// <summary>
        /// 构造接收消息体
        /// </summary>
        /// <returns></returns>
        public (RpcGlobalMessageBase<object> messageBase, Dictionary<string, string> traceHeaders) GetReceiveMessage(object message)
        {
            if (message is IHttpContent)
            {
                var buf = ((IHttpContent)message).Content;
                var jsonstr = buf.ToString(Encoding.UTF8);
                Dictionary<string, string> traceHeaders = default;
                if (message is IFullHttpRequest)
                {
                    var headers = ((IFullHttpRequest)message).Headers;
                    traceHeaders = TraceHeaderHelper.GetTraceHeaders(headers);
                }
                return (_serialize.DeserializesJson<RpcGlobalMessageBase<object>>(jsonstr), traceHeaders);
            }
            else if (message is RpcGlobalMessageBase<object>)
            {
                return ((RpcGlobalMessageBase<object>)message,null);
            }
            return default;
        }
    }
}
