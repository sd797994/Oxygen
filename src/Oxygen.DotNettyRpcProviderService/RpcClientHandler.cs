using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oxygen.Common.Logger;

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
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer buffer)
                {
                    int length = buffer.ReadableBytes;
                    var array = new byte[length];
                    buffer.GetBytes(buffer.ReaderIndex, array, 0, length);
                    _hander?.Invoke(array);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("客户端回调处理异常: " + e.Message);
            }
        }
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError("客户端回调异常: " + exception.Message);
            context.CloseAsync();
        }
    }
}
