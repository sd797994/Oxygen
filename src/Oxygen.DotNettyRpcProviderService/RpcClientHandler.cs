using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using System;
using System.Text;

namespace Oxygen.DotNettyRpcProviderService
{
    /// <summary>
    /// 客户端回调处理类
    /// </summary>
    public class RpcClientHandler : ChannelHandlerAdapter
    {
        private event ReceiveHander _hander;
        private readonly IOxygenLogger _logger;
        public RpcClientHandler(IOxygenLogger logger, ReceiveHander hander)
        {
            _logger = logger;
            _hander = hander;
        }
        /// <summary>
        /// 从tcp管道接受消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is RpcGlobalMessageBase<object>)
                {
                    _hander?.Invoke((RpcGlobalMessageBase<object>)message);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("客户端回调处理异常: " + e.Message);
            }
        }
        /// <summary>
        /// tcp管道异常处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError("客户端回调异常: " + exception.Message);
            context.CloseAsync();
        }
    }
}
