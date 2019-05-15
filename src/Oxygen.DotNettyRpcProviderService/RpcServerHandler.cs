using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oxygen.Common.Logger;
using Oxygen.IServerProxyFactory;
using System;
using System.Linq;

namespace Oxygen.DotNettyRpcProviderService
{
    /// <summary>
    /// 服务端消息处理类
    /// </summary>
    public class RpcServerHandler : ChannelHandlerAdapter
    {
        private readonly IOxygenLogger _logger;
        private readonly ILocalProxyGenerator _localProxyGenerator;
        public RpcServerHandler(IOxygenLogger logger, ILocalProxyGenerator localProxyGenerator)
        {
            _logger = logger;
            _localProxyGenerator = localProxyGenerator;
        }
        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer buffer)
                {
                    var data = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(data);
                    var localHanderResult = await _localProxyGenerator.Invoke(data);
                    if (localHanderResult != null && localHanderResult.Any())
                    {
                        await context.WriteAsync(Unpooled.WrappedBuffer(localHanderResult));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("服务端消息处理异常: " + e.Message);
            }
        }

        /// <summary>
        /// tcp消息回调
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError("服务端消息处理异常: " + exception.Message);
            context.CloseAsync();
        }
    }
}
