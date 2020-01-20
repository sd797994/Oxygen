using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.Multipart;
using DotNetty.Common;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Linq;
using System.Text;

namespace Oxygen.DotNettyRpcProviderService
{
    /// <summary>
    /// 服务端消息处理类
    /// </summary>
    public class RpcServerHandler : ChannelHandlerAdapter
    {
        private readonly IOxygenLogger _logger;
        private readonly ILocalProxyGenerator _localProxyGenerator;
        private readonly ISerialize _serialize;
        private readonly ProtocolMessageBuilder protocolMessageBuilder;
        public RpcServerHandler(IOxygenLogger logger, ILocalProxyGenerator localProxyGenerator, ISerialize serialize)
        {
            _logger = logger;
            _localProxyGenerator = localProxyGenerator;
            _serialize = serialize;
            protocolMessageBuilder = new ProtocolMessageBuilder(_serialize);
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
                var messageobj = protocolMessageBuilder.GetReceiveMessage(message);
                var localHanderResult = await _localProxyGenerator.Invoke(messageobj);
                if (localHanderResult != null)
                {
                    await context.WriteAndFlushAsync(protocolMessageBuilder.GetServerSendMessage(localHanderResult));
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
