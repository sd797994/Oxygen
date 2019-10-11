using DotNetty.Buffers;
using DotNetty.Codecs;
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
using System.Threading.Tasks;

namespace Oxygen.DotNettyRpcProviderService
{
    /// <summary>
    /// 服务端消息服务
    /// </summary>
    public class RpcServerProvider : IRpcServerProvider
    {
        private readonly IOxygenLogger _logger;
        private readonly ILocalProxyGenerator _localProxyGenerator;
        private readonly ISerialize _serialize;
        #region dotnetty相关
        IEventLoopGroup _bossGroup;
        IEventLoopGroup _workerGroup;
        ServerBootstrap _bootstrap;
        IChannel boundChannel;
        #endregion

        public RpcServerProvider(IOxygenLogger logger,ILocalProxyGenerator localProxyGenerator, ISerialize serialize)
        {
            _logger = logger;
            _localProxyGenerator = localProxyGenerator;
            _serialize = serialize;
        }

        /// <summary>
        /// 启动tcp服务
        /// </summary>
        /// <returns></returns>
        public async Task<IPEndPoint> OpenServer()
        {
            var dispatcher = new DispatcherEventLoopGroup();
            _bossGroup = dispatcher;
            _workerGroup = new WorkerEventLoopGroup(dispatcher);
            _bootstrap = new ServerBootstrap()
                    .Channel<TcpServerChannel>()
                    .Group(_bossGroup, _workerGroup)
                    .Option(ChannelOption.SoBacklog, 100)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.ConnectTimeout, new TimeSpan(0, 0, 5))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;
                        pipeline.AddLast(new LengthFieldPrepender(4));
                        pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                        pipeline.AddLast(new MessageDecoder<RpcGlobalMessageBase<object>>(_serialize));
                        pipeline.AddLast(new MessageEncoder<object>(_serialize));
                        pipeline.AddLast(new RpcServerHandler(_logger, _localProxyGenerator));
                    }));
            var port = OxygenSetting.ServerPort ?? GlobalCommon.GetFreePort();
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
