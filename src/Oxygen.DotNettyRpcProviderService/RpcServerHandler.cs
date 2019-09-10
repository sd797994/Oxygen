using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
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
        private readonly IGlobalCommon _globalCommon;
        public RpcServerHandler(IOxygenLogger logger, ILocalProxyGenerator localProxyGenerator, IGlobalCommon globalCommon)
        {
            _logger = logger;
            _localProxyGenerator = localProxyGenerator;
            _globalCommon = globalCommon;
        }
        /// <summary>
        /// 从tcp管道接受消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer buffer)
                {
                    var data = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(data);
                    var localHanderResult = await _localProxyGenerator.Invoke(_globalCommon.RsaDecrypt(data));
                    if (localHanderResult != null && localHanderResult.Any())
                    {
                        await context.WriteAsync(Unpooled.WrappedBuffer(_globalCommon.RsaEncryp(localHanderResult)));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("服务端消息处理异常: " + e.Message);
            }
        }

        /// <summary>
        /// tcp消息回发
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        /// <summary>
        /// tcp管道异常处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError("服务端消息处理异常: " + exception.Message);
            context.CloseAsync();
        }
    }
}
