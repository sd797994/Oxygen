using Oxygen.IServerFlowControl;
using Polly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Oxygen.ServerFlowControl
{
    /// <summary>
    /// 断路策略接口
    /// </summary>
    public interface ICircuitPolicyProvider
    {
        /// <summary>
        /// 通过配置组装熔断策略
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathName"></param>
        /// <param name="config"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        IAsyncPolicy<T> BuildPolicy<T>(string key, ServiceConfigureInfo config, IPEndPoint endpoint);

        /// <summary>
        /// 写入请求成功的时间
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="point"></param>
        void PushTimeInReq(string key, IPEndPoint point);
    }
}
