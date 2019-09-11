using Oxygen.CommonTool.Logger;
using Oxygen.IServerFlowControl;
using System;
using System.Linq;
using System.Net;

namespace Oxygen.ServerFlowControl
{
    /// <summary>
    /// 断路器
    /// </summary>
    public class EndPointCircuitBreaker : IEndPointCircuitBreaker
    {
        private readonly IEndPointConfigureManager _endPointConfigure;
        private readonly ITokenBucket _tokenBucket;
        private readonly IOxygenLogger _logger;
        public EndPointCircuitBreaker(IEndPointConfigureManager endPointConfigure, ITokenBucket tokenBucket, IOxygenLogger logger)
        {
            _endPointConfigure = endPointConfigure;
            _tokenBucket = tokenBucket;
            _logger = logger;
        }
        /// <summary>
        /// 检查服务断路状态
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="serviceInfo"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public bool CheckCircuitByEndPoint(string key, IPEndPoint clientEndPoint, ServiceConfigureInfo serviceInfo, out IPEndPoint addr)
        {
            //根据配置抛弃断路状态地址
            var useAddr = serviceInfo.EndPoints.Where(x => x.BreakerTime == null || (x.BreakerTime != null && x.BreakerTime.Value.AddSeconds(serviceInfo.DefBreakerRetryTimeSec) <= DateTime.Now)).ToList();
            //若全部熔断
            if (!useAddr.Any())
            {
                _logger.LogError("服务被熔断,无法提供服务");
                addr = null;
                return false;
            }
            else
            {
                //负载均衡有效地址
                addr = _endPointConfigure.GetServieByLoadBalane(useAddr, clientEndPoint, LoadBalanceType.IPHash);
                //初始化令牌桶并判断是否限流
                _tokenBucket.InitTokenBucket(serviceInfo.DefCapacity, serviceInfo.DefRateLimit);
                return _tokenBucket.Grant(key, serviceInfo);
            }
        }
    }
}
