using System;
using System.Net;

namespace Oxygen.IRpcProviderService
{
    /// <summary>
    /// RPC通用消息父类
    /// </summary>
    public class RpcGlobalMessageBase<T>
    {
        public IPEndPoint CustomerIp { get; set; }
        public Guid TaskId { get; set; }
        public string Path { get; set; }
        public T Message { get; set; }
    }
}
