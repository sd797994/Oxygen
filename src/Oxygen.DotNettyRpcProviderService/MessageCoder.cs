using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.DotNettyRpcProviderService
{
    public class MessageDecoder<T> : MessageToMessageDecoder<IByteBuffer>
    {
        private ISerialize _serialize { get; }


        public MessageDecoder(ISerialize serialize)
        {
            _serialize = serialize;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            var len = message.ReadableBytes;
            var array = new byte[len];
            message.GetBytes(message.ReaderIndex, array, 0, len);
            output.Add(_serialize.Deserializes<T>(array));
        }
    }
    public class MessageEncoder<T> : MessageToByteEncoder<RpcGlobalMessageBase<T>>
    {
        private ISerialize _serialize { get; }

        public MessageEncoder(ISerialize serialize)
        {
            _serialize = serialize;
        }
        protected override void Encode(IChannelHandlerContext context, RpcGlobalMessageBase<T> message, IByteBuffer output)
        {
            output.WriteBytes(_serialize.Serializes(message));
        }
    }
}
