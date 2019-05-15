using System;

namespace Oxygen.IRpcProviderService
{
    /// <summary>
    /// RPC通用通信父类
    /// </summary>
    public class RpcGlobalMessageBase<T>
    {
        public Guid TaskId { get; set; }
        public string Path { get; set; }
        public T Message { get; set; }
    }
}
