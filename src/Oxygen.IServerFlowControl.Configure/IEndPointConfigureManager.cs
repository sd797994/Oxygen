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
        /// 判断配置节是否存在
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <returns></returns>
        Task<bool> CheckBreakerConfigureAny(string flowControlCfgKey);
        /// <summary>
        /// 获取服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        Task<ServiceConfigureInfo> GetBreakerConfigure(string flowControlCfgKey);

        /// <summary>
        /// 强制熔断无法连通的EndPoint
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        /// <param name="breakEndPoint"></param>
        Task ForcedCircuitBreakEndPoint(string flowControlCfgKey, IPEndPoint breakEndPoint);

        /// <summary>
        /// 更新熔断结束的配置文件
        /// </summary>
        /// <param name="servcieInfo"></param>
        Task CleanBreakTimes(string flowControlCfgKey);

        /// <summary>
        /// 删除配置节所有下属节点
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        Task RemoveAllNode(string flowControlCfgKey);
        /// <summary>
        /// 根据服务路由更新配置节
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="addrs"></param>
        Task ReflushConfigureEndPoint(string flowControlCfgKey, List<IPEndPoint> addrs);

        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="clientIp"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IPEndPoint GetServieByLoadBalane(List<FlowControlEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash, string flowControlCfgKey = null);
        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="clientIp"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IPEndPoint GetServieByLoadBalane(List<IPEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash);
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
        /// 修改最小连接数
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="address"></param>
        /// <param name="IsPlus"></param>
        Task ChangeConnectCount(string flowControlCfgKey, IPEndPoint address, bool IsPlus);
        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="address"></param>
        Task ReflushCache(string flowControlCfgKey, object entity);
        /// <summary>
        /// 服务器端初始化并更新配置节到缓存
        /// </summary>
        /// <param name="types"></param>
        Task SetCacheFromServices();
        /// <summary>
        /// 返回限流桶配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        Task<TokenBucketInfo> GetOrAddTokenBucket(string key, int defCapacity);
        /// <summary>
        /// 更新令牌时间戳
        /// </summary>
        /// <param name="bucketInfo"></param>
        /// <param name="Capacity"></param>
        /// <param name="Rate"></param>
        void UpdateTokens(TokenBucketInfo bucketInfo, long Capacity, long Rate);
        /// <summary>
        /// 更新令牌数量并发布
        /// </summary>
        /// <param name="key"></param>
        /// <param name="bucketInfo"></param>
        Task UpdateTokenBucket(string key, TokenBucketInfo bucketInfo);
    }
}
