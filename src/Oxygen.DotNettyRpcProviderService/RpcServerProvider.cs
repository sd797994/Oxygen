using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Oxygen.DotNettyRpcProviderService
{
    /// <summary>
    /// 服务端消息服务
    /// </summary>
    public class RpcServerProvider : IRpcServerProvider
    {
        private readonly IOxygenLogger _logger;
        #region dotnetty相关
        IEventLoopGroup _bossGroup;
        IEventLoopGroup _workerGroup;
        ServerBootstrap _bootstrap;
        IChannel boundChannel;
        #endregion

        public RpcServerProvider(IOxygenLogger logger,ILocalProxyGenerator localProxyGenerator, ISerialize serialize)
        {
            _logger = logger;
            _bootstrap = new BootstrapFactory(logger, serialize).CreateServerBootstrap(localProxyGenerator);
        }

        /// <summary>
        /// 启动tcp服务
        /// </summary>
        /// <returns></returns>
        public async Task<IPEndPoint> OpenServer(Action<object> action)
        {
            var port = OxygenSetting.ServerPort;
            boundChannel = await _bootstrap.BindAsync(port);
            _logger.LogInfo($"bind tcp 0.0.0.0:{port} to listen");
            return new IPEndPoint(GlobalCommon.GetMachineIp(), port);
        }
        /// <summary>
        /// 关闭tcp服务
        /// </summary>
        /// <returns></returns>
        public async Task CloseServer()
        {
            await boundChannel.CloseAsync();
            await _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            await _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
    }
}
