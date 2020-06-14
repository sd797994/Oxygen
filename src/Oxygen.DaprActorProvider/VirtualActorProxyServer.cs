using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.CsharpClientAgent;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{

    public class VirtualActorProxyServer : IVirtualProxyServer
    {
        static Lazy<ISerialize> serialize = new Lazy<ISerialize>(() => OxygenIocContainer.Resolve<ISerialize>());
        static Lazy<IOxygenLogger> logger = new Lazy<IOxygenLogger>(() => OxygenIocContainer.Resolve<IOxygenLogger>());
        string method { get; set; }
        string actorType { get; set; }
        string actorId { get; set; }
        HttpClient client;
        public VirtualActorProxyServer(string method,string actorType,string actorId)
        {
            this.method = method;
            this.actorType = actorType;
            this.actorId = actorId;
            this.InputType = RpcInterfaceType.ActorTypes.Value.FirstOrDefault(x => x.interfaceType.Name.Equals($"I{actorType}")).interfaceType.GetMethod(method).GetParameters()[0].ParameterType;
            this.client = new HttpClient();
        }
        public Type InputType { get; set; }

        public void Init(string serverName, string pathName, Type inputType, Type returnType)
        {

        }

        public async Task<object> SendAsync(object input)
        {
            var responseMessage = await client.SendAsync(GetClientSendMessage(input, actorType, actorId, method));
            if (responseMessage != null && responseMessage.StatusCode == HttpStatusCode.OK)
            {
                return serialize.Value.DeserializesJson<object>(await responseMessage.Content.ReadAsStringAsync());
            }
            else
            {
                logger.Value.LogError($"客户端调用actor请求异常,状态码：{responseMessage?.StatusCode}");
            }
            return default;
        }

        HttpRequestMessage GetClientSendMessage(object input,string actorType,string actorId,string method)
        {
            var json = serialize.Value.SerializesJson(input);
            actorId = string.IsNullOrEmpty(actorId) ? (serialize.Value.DeserializesJson(InputType, json) as ActorModel).Key : actorId;
            var url = $"http://localhost:3500/v1.0/actors/{actorType}/{actorId}/method/{method}";
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Version = new Version(1, 1) };
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }
    }
}
