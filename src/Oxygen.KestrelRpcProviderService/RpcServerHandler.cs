using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.KestrelRpcProviderService
{
    public class RpcServerHandler
    {
        private ISerialize _serialize;
        private IOxygenLogger _logger;
        private readonly ProtocolMessageBuilder protocolMessageBuilder;
        private readonly ILocalProxyGenerator _localProxyGenerator;
        public RpcServerHandler(ISerialize serialize, ILocalProxyGenerator localProxyGenerator, IOxygenLogger logger)
        {
            _serialize = serialize;
            _localProxyGenerator = localProxyGenerator;
            _logger = logger;
            protocolMessageBuilder = new ProtocolMessageBuilder(_serialize);
        }
        public void BuildHandler(IApplicationBuilder app)
        {
            app.MapWhen(contxt => contxt.Request.Method == "POST", appbuilder => appbuilder.Run(async http => await handle(http)));
        }
        private async Task handle(HttpContext http)
        {
            try
            {
                var messageobj = await protocolMessageBuilder.GetReceiveMessage(http);
                var localHanderResult = await _localProxyGenerator.Invoke(messageobj);
                if (localHanderResult != null)
                {
                    byte[] json = _serialize.Serializes(localHanderResult);
                    await http.Response.Body.WriteAsync(json, 0, json.Length);
                }
                else
                {
                    await http.Response.Body.WriteAsync(new byte[] { }, 0, 0);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("服务端消息处理异常: " + e.Message);
                await http.Response.Body.WriteAsync(new byte[] { }, 0, 0);
            }
        }
    }
}
