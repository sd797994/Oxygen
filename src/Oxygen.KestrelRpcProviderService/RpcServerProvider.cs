using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.KestrelRpcProviderService
{
    /// <summary>
    /// kestrel服务端提供者
    /// </summary>
    public class RpcServerProvider : IRpcServerProvider
    {
        public IWebHost Host { get; private set; }
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        private readonly ILocalProxyGenerator _localProxyGenerator;
        public RpcServerProvider(IOxygenLogger logger,ISerialize serialize, ILocalProxyGenerator localProxyGenerator)
        {
            _logger = logger;
            _serialize = serialize;
            _localProxyGenerator = localProxyGenerator;
        }

        /// <summary>
        /// 启动kestrel服务器
        /// </summary>
        /// <returns></returns>
        public async Task<IPEndPoint> OpenServer()
        {
            var port = OxygenSetting.ServerPort;
            var builder = new WebHostBuilder()
                   .UseKestrel(options =>
                   {
                       options.Listen(IPAddress.Any, port, listenOptions =>
                       {
                           listenOptions.Protocols = HttpProtocols.Http2;
                       });
                   })
                   .Configure(app =>new RpcServerHandler(_serialize, _localProxyGenerator, _logger).BuildHandler(app));
            Host = builder.Build();
            await Host.StartAsync();
            _logger.LogInfo($"bind tcp 0.0.0.0:{port} to listen");
            return new IPEndPoint(GlobalCommon.GetMachineIp(), port);
        }

        /// <summary>
        /// 关闭kestrel服务器
        /// </summary>
        /// <returns></returns>
        public async Task CloseServer()
        {
            await Host.StopAsync();
        }
    }
}
