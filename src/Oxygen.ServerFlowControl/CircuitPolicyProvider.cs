using Oxygen.CommonTool.Logger;
using Oxygen.IServerFlowControl;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Oxygen.ServerFlowControl
{
    public class CircuitPolicyProvider: ICircuitPolicyProvider
    {
        private static ConcurrentDictionary<string, List<DateTime>> tmpReqTime;
        private readonly IEndPointConfigureManager _endPointConfigure;
        private readonly IOxygenLogger _logger;

        public CircuitPolicyProvider(IEndPointConfigureManager endPointConfigure, IOxygenLogger logger)
        {
            _endPointConfigure = endPointConfigure;
            tmpReqTime = tmpReqTime ?? new ConcurrentDictionary<string, List<DateTime>>();
            _logger = logger;
        }
        /// <summary>
        /// 通过配置组装熔断策略
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public IAsyncPolicy<T> BuildPolicy<T>(string key, ServiceConfigureInfo config, IPEndPoint endpoint)
        {
            var address = config.EndPoints.FirstOrDefault(x => x.GetEndPoint().Equals(endpoint));
            //定义默认策略
            IAsyncPolicy<T> defPolicy = Policy<T>.Handle<Exception>().FallbackAsync((CancellationToken cancelToken) =>
            {
                //请求失败，更新配置节并进行回退
                address.ThresholdBreakeTimes += 1;
                tmpReqTime.TryGetValue($"{key}{endpoint.Address.ToString()}", out List<DateTime> time);
                time = time ?? new List<DateTime>();
                //若错误次数超过阈值或者阈值比例则触发熔断
                if (address.ThresholdBreakeTimes >= config.DefThresholdBreakeTimes || time.Count() == 0 || (time.Count() > 0 && (double)config.DefThresholdBreakeRatePerSec >= (double)address.ThresholdBreakeTimes / (double)time.Count(y => y >= DateTime.Now.AddSeconds(1))))
                {
                    _logger.LogError($"地址{address.GetEndPoint().ToString()}超过熔断阈值，强制熔断。详细信息{{当前IP熔断次数:{address.ThresholdBreakeTimes},成功请求次数:{time.Count()},熔断比率{(double)address.ThresholdBreakeTimes / (double)time.Count(y => y >= DateTime.Now.AddSeconds(1))}}}");
                    address.BreakerTime = DateTime.Now;
                }
                _endPointConfigure.UpdateBreakerConfigure(key, config);
                _logger.LogError($"地址{address.GetEndPoint().ToString()}请求出错,执行回退");
                return default;
            });
            //定义重试策略
            if (config.DefRetryTimes > 0 && config.DefRetryTimesSec == 0)
            {
                defPolicy = defPolicy.WrapAsync(Policy.Handle<Exception>().RetryAsync(config.DefRetryTimes, (exception, retryCount) =>
                {
                    _logger.LogError($"地址{address.GetEndPoint().ToString()}调用重试{retryCount}次，异常原因:{exception.Message}");
                }));
            }
            else if (config.DefRetryTimes > 0 && config.DefRetryTimesSec > 0)
            {
                var timespan = new TimeSpan[config.DefRetryTimes];
                for (var i = 0; i < config.DefRetryTimes; i++)
                {
                    timespan[i] = new TimeSpan(0, 0, config.DefRetryTimesSec);
                }
                defPolicy = defPolicy.WrapAsync(Policy.Handle<Exception>().WaitAndRetryAsync(timespan, (exception, TimeSpan, retryCount, Context) =>
                {
                    _logger.LogError($"地址{address.GetEndPoint().ToString()}调用{TimeSpan.TotalSeconds}秒后重试第{retryCount}次，异常原因:{exception.Message}");
                }));
            }
            //定义超时策略
            if (config.DefTimeOutBreakerSec > 0)
            {
                defPolicy = defPolicy.WrapAsync(Policy.TimeoutAsync(config.DefTimeOutBreakerSec, TimeoutStrategy.Pessimistic, (Context, TimeSpan, Task) =>
                {
                    _logger.LogError($"地址{address.GetEndPoint().ToString()}调用超时:{TimeSpan.TotalSeconds}秒");
                    return default;
                }));
            }
            return defPolicy;
        }
        /// <summary>
        /// 写入请求成功的时间
        /// </summary>
        public void PushTimeInReq(string key, IPEndPoint point)
        {
            var tmpRetKey = $"{key}{point.Address.ToString()}";
            if (tmpReqTime.TryGetValue(tmpRetKey, out List<DateTime> time))
            {
                time.Add(DateTime.Now);
                tmpReqTime.TryRemove(tmpRetKey, out List<DateTime> old);
                tmpReqTime.TryAdd(tmpRetKey, time.Where(x => x.AddSeconds(10) > DateTime.Now).ToList());//丢弃10秒以前的数据保证字典不会过大
            }
            else
            {
                tmpReqTime.TryAdd(tmpRetKey, new List<DateTime>() { DateTime.Now });
            }

        }
    }
}
