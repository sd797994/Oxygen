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
        /// <summary>
        /// 签名字段
        /// </summary>
        private string sign { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="key"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public void Sign(string sign)
        {
            this.sign = sign;
        }
        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="key"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public bool CheckSign(string sign)
        {
            return this.sign.Equals(sign);
        }

        public HttpStatusCode code { get; set; }
    }
}
