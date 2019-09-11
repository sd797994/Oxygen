using System;
using System.Net;

namespace Oxygen.IRpcProviderService
{
    /// <summary>
    /// RPC通用消息父类
    /// </summary>
    public class RpcGlobalMessageBase<T>
    {
        /// <summary>
        /// 客户端IP
        /// </summary>
        public IPEndPoint CustomerIp { get; set; }
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid TaskId { get; set; }
        /// <summary>
        /// 请求服务路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 请求消息体
        /// </summary>
        public T Message { get; set; }
    }
}
