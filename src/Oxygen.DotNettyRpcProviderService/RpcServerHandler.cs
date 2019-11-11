using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
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
        /// <summary>
        /// 从tcp管道接受消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is RpcGlobalMessageBase<object>)
                {
                    var localHanderResult = await _localProxyGenerator.Invoke((RpcGlobalMessageBase<object>)message);
                    if (localHanderResult != null)
                    {
                        await context.WriteAndFlushAsync(localHanderResult);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("服务端消息处理异常: " + e.Message);
            }
        }

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
