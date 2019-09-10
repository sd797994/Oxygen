using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerFlowControl
{
    /// <summary>
    /// 流控中心
    /// </summary>
    public interface IFlowControlCenter
    {
        /// <summary>
        /// 根据服务名返回IP地址
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<(IPEndPoint endPoint, ServiceConfigureInfo configureInfo)> GetFlowControlEndPointByServicePath(string serviceName, string key, IPEndPoint clientIp);

        /// <summary>
        /// 根据断路策略
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceName"></param>
        /// <param name="path"></param>
        /// <param name="endPoint"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        Task<T> ExcuteAsync<T>(string key, IPEndPoint endPoint, string flowControlCfgKey, ServiceConfigureInfo configureInfo, Func<Task<T>> func) where T : class;
    }
}
