using System.Collections.Generic;
using System.Net;

namespace Oxygen.IServerFlowControl
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
        ServiceConfigureInfo GetOrAddBreakerConfigure(string pathName);

        /// <summary>
        /// 更新服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        void UpdateBreakerConfigure(string pathName, ServiceConfigureInfo servcieInfo);

        /// <summary>
        /// 强制熔断无法连通的EndPoint
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        /// <param name="breakEndPoint"></param>
        void ForcedCircuitBreakEndPoint(string pathName, ServiceConfigureInfo servcieInfo, IPEndPoint breakEndPoint);

        /// <summary>
        /// 更新熔断结束的配置文件
        /// </summary>
        /// <param name="servcieInfo"></param>
        void CleanBreakTimes(ServiceConfigureInfo servcieInfo);

        /// <summary>
        /// 根据服务路由更新配置节
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="addrs"></param>
        void ReflushConfigureEndPoint(ServiceConfigureInfo serviceInfo, List<FlowControlEndPoint> addrs);

        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="clientIp"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IPEndPoint GetServieByLoadBalane(List<FlowControlEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash);

        /// <summary>
        /// 修改最小连接数
        /// </summary>
        /// <param name="lbEndPoints"></param>
        /// <param name="address"></param>
        /// <param name="IsPlus"></param>
        void ChangeConnectCount(List<FlowControlEndPoint> lbEndPoints, IPEndPoint address, bool IsPlus);

        /// <summary>
        /// 客户端注册熔断配置节缓存订阅
        /// </summary>
        void SubscribeAllService();
        /// <summary>
        /// 服务器端初始化并更新配置节到缓存
        /// </summary>
        /// <param name="types"></param>
        void SetCacheFromServices();
        /// <summary>
        /// 返回限流桶配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        TokenBucketInfo GetOrAddTokenBucket(string key, int defCapacity);
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
        void UpdateTokenBucket(string key, TokenBucketInfo bucketInfo);
    }
}
