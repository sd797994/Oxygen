using Oxygen.CommonTool.Logger;
using Oxygen.IServerFlowControl;
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        public async Task<IPEndPoint> CheckCircuitByEndPoint(ServiceConfigureInfo configure, IPEndPoint clientEndPoint)
        {
            //根据配置抛弃断路状态地址
            var useAddr = configure.GetEndPoints().Where(x => x.BreakerTime == null || (x.BreakerTime != null && x.BreakerTime.Value.AddSeconds(configure.DefBreakerRetryTimeSec) <= DateTime.Now)).ToList();
            //若全部熔断
            if (!useAddr.Any())
            {
                _logger.LogError("服务被熔断,无法提供服务");
                return null;
            }
            else
            {
                //负载均衡有效地址
                var addr = _endPointConfigure.GetServieByLoadBalane(useAddr, clientEndPoint, LoadBalanceType.IPHash, configure);
                //初始化令牌桶并判断是否限流
                _tokenBucket.InitTokenBucket(configure.DefCapacity, configure.DefRateLimit);
                if (await _tokenBucket.Grant(configure))
                {
                    return addr;
                }
                return null;
            }
        }
    }
}
