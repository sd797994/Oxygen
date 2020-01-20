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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Oxygen.DotNettyRpcProviderService
{
    public class BootstrapFactory
    {
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        public BootstrapFactory(IOxygenLogger logger, ISerialize serialize)
        {
            _logger = logger;
            _serialize = serialize;
        }
        /// <summary>
        /// 创建客户端启动器
        /// </summary>
        /// <param name="receiveHander"></param>
        /// <returns></returns>
        public Bootstrap CreateClientBootstrap(ReceiveHander receiveHander)
        {
            IEventLoopGroup group;
            var bootstrap = new Bootstrap();
            group = new EventLoopGroup();
            switch (OxygenSetting.ProtocolType)
            {
                case EnumProtocolType.HTTP11:
                    bootstrap.Channel<TcpChannel>();
                    bootstrap
                                .Group(group)
                        .Option(ChannelOption.SoBacklog, 8192)
                            .Handler(new ActionChannelInitializer<IChannel>(channel =>
                            {
                                IChannelPipeline pipeline = channel.Pipeline;
                                pipeline.AddLast(new HttpClientCodec());
                                pipeline.AddLast(new HttpObjectAggregator(1024 * 10 * 1024));
                                pipeline.AddLast(new HttpContentDecompressor());
                                pipeline.AddLast("handler", new RpcClientHandler(_logger, _serialize, receiveHander));
                            }));
                    break;
                case EnumProtocolType.TCP:
                default:
                    bootstrap.Channel<TcpChannel>();
                    bootstrap
                                .Group(group)
                                .Option(ChannelOption.TcpNodelay, true)
                                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                                .Option(ChannelOption.ConnectTimeout, new TimeSpan(0, 0, 5))
                                .Handler(new ActionChannelInitializer<IChannel>(ch =>
                                {
                                    var pipeline = ch.Pipeline;
                                    pipeline.AddLast(new LengthFieldPrepender(4));
                                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                                    pipeline.AddLast(new MessageDecoder<RpcGlobalMessageBase<object>>(_serialize));
                                    pipeline.AddLast(new MessageEncoder<object>(_serialize));
                                    pipeline.AddLast(new RpcClientHandler(_logger, _serialize, receiveHander));
                                }));
                    break;
            }
            return bootstrap;
        }
        /// <summary>
        /// 创建服务端启动器
        /// </summary>
        /// <param name="localProxyGenerator"></param>
        /// <returns></returns>
        public ServerBootstrap CreateServerBootstrap(ILocalProxyGenerator localProxyGenerator)
        {
            var dispatcher = new DispatcherEventLoopGroup();
            var _bossGroup = dispatcher;
            var _workerGroup = new WorkerEventLoopGroup(dispatcher);
            var _bootstrap = new ServerBootstrap().Channel<TcpServerChannel>()
                      .Group(_bossGroup, _workerGroup);
            switch (OxygenSetting.ProtocolType)
            {
                case EnumProtocolType.HTTP11:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        _bootstrap.Option(ChannelOption.SoReuseport, true)
                                .ChildOption(ChannelOption.SoReuseaddr, true);
                    }
                    _bootstrap
                        .Option(ChannelOption.SoBacklog, 8192)
                            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                            {
                                IChannelPipeline pipeline = channel.Pipeline;
                                pipeline.AddLast("codec", new HttpServerCodec());
                                pipeline.AddLast(new HttpObjectAggregator(1024 * 10 * 1024));
                                pipeline.AddLast(new HttpContentCompressor());
                                pipeline.AddLast("handler", new RpcServerHandler(_logger, localProxyGenerator, _serialize));
                            }));
                    break;
                case EnumProtocolType.TCP:
                default:
                    _bootstrap.Channel<TcpServerChannel>()
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
                        pipeline.AddLast(new RpcServerHandler(_logger, localProxyGenerator, _serialize));
                    }));
                    break;
            }
            return _bootstrap;
        }
    }
}
