using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Oxygen.IServerFlowControl.Configure
{
    /// <summary>
    /// 流控配置管理器
    /// </summary>
    public interface IEndPointConfigureManager
    {
        /// <summary>
        /// 获取服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        Task<ServiceConfigureInfo> GetBreakerConfigure(string flowControlCfgKey);
        /// <summary>
        /// 更新服务配置节
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="servcieInfo"></param>
        Task UpdateBreakerConfigure(string flowControlCfgKey, ServiceConfigureInfo servcieInfo);
        /// <summary>
        /// 服务端初始化配置节
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="servcieInfo"></param>
        /// <returns></returns>
        Task InitBreakerConfigure(string flowControlCfgKey, ServiceConfigureInfo servcieInfo);

        /// <summary>
        /// 强制熔断无法连通的EndPoint
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        /// <param name="breakEndPoint"></param>
        Task ForcedCircuitBreakEndPoint(string flowControlCfgKey, ServiceConfigureInfo configure, IPEndPoint breakEndPoint);

        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="clientIp"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IPEndPoint GetServieByLoadBalane(List<FlowControlEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash, ServiceConfigureInfo configure = null);
        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="clientIp"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IPEndPoint GetServieByLoadBalane(List<IPEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash);
        /// <summary>
        /// 服务器端初始化并更新配置节到缓存
        /// </summary>
        /// <param name="types"></param>
        Task SetCacheFromServices();
    }
}
