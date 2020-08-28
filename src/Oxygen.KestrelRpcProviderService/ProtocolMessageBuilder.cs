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
using System.Threading.Tasks;

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
            byte[] json = _serialize.Serializes(sendMessage);
            Version clientVersion = default;
            switch (OxygenSetting.ProtocolType)
            {
                case EnumProtocolType.HTTP11:
                    clientVersion = new Version(1, 1);
                    break;
                case EnumProtocolType.HTTP2:
                    clientVersion = new Version(2, 0);
                    break;
            }
            string url;
            switch (OxygenSetting.MeshType)
            {
                case EnumMeshType.Dapr:
                    url = $"http://localhost:3500/v1.0/invoke/{serverName}/method/{pathName}";
                    break;
                default:
                    url = $"http://{serverName}:{OxygenSetting.ServerPort}";
                    break;
            }
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Version = clientVersion };
            request.Content = new ByteArrayContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-msgpack");
            request.Content.Headers.ContentLength = json.Length;
            TraceHeaderHelper.BuildTraceHeader(request.Content.Headers, traceHeaders);
            return request;
        }
        public async Task<(RpcGlobalMessageBase<object> messageBase, Dictionary<string, string> traceHeaders)> GetReceiveMessage(HttpContext message)
        {
            using (var buffer = new MemoryStream())
            {
                await message.Request.Body.CopyToAsync(buffer);
                byte[] bytes = buffer.ToArray();
                buffer.Write(bytes, 0, Convert.ToInt32(buffer.Length));
                var result = new RpcGlobalMessageBase<object>();
                //根据header contenttype选择是反序列化json还是反序列化byte
                if (message.Request.ContentType != "application/json")
                    result = _serialize.Deserializes<RpcGlobalMessageBase<object>>(bytes);
                else
                {
                    result.Path = message.Request.Path.Value;
                    result.Message = _serialize.DeserializesJson<object>(Encoding.Default.GetString(bytes));
                }
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
