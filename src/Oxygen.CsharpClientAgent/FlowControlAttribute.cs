using System;

namespace Oxygen.CsharpClientAgent
{
    /// <summary>
    /// 服务接口断路控制
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FlowControlAttribute : Attribute
    {
        public FlowControlAttribute(string FunctionName,bool DefOpenCache = true, object DefCacheData = null, int DefCapacity = 2000, int DefRateLimit = 1, int DefThresholdBreakeTimes = 10, double DefThresholdBreakeRatePerSec = 0.5F,
            int DefRetryTimes = 3, int DefRetryTimesSec = 5, int DefBreakerRetryTimeSec = 300, int DefTimeOutBreakerSec = 5)
        {
            this.FunctionName = FunctionName;
            this.DefOpenCache = DefOpenCache;
            this.DefCacheData = DefCacheData;
            this.DefCapacity = DefCapacity;
            this.DefRateLimit = DefRateLimit;
            this.DefThresholdBreakeTimes = DefThresholdBreakeTimes;
            this.DefThresholdBreakeRatePerSec = DefThresholdBreakeRatePerSec < 0 || DefThresholdBreakeRatePerSec > 1 ? 1 : DefThresholdBreakeRatePerSec;
            this.DefRetryTimes = DefRetryTimes;
            this.DefRetryTimesSec = DefRetryTimesSec;
            this.DefBreakerRetryTimeSec = DefBreakerRetryTimeSec;
            this.DefTimeOutBreakerSec = DefTimeOutBreakerSec;
        }
        /// <summary>
        /// 服务方法名称
        /// </summary>
        public string FunctionName { get; set; }
        /// <summary>
        /// 默认启用缓存
        /// </summary>
        public bool DefOpenCache { get; set; }

        /// <summary>
        /// 默认缓存对象
        /// </summary>
        public object DefCacheData { get; set; }

        /// <summary>
        /// 令牌桶默认最大容量(枚)
        /// </summary>
        public int DefCapacity { get; set; }

        /// <summary>
        /// 令牌生成桶默认间隔(秒)
        /// </summary>
        public int DefRateLimit { get; set; }
        /// <summary>
        /// 默认熔断阈值
        /// </summary>
        public int DefThresholdBreakeTimes { get; set; }

        /// <summary>
        /// 默认熔断阈值比率
        /// </summary>
        public double DefThresholdBreakeRatePerSec { get; set; }
        /// <summary>
        /// 默认重试次数
        /// </summary>
        public int DefRetryTimes { get; set; }
        /// <summary>
        /// 默认单次请求重试时间(秒)
        /// </summary>
        public int DefRetryTimesSec { get; set; }

        /// <summary>
        /// 默认熔断后再次请求重试时间(秒)
        /// </summary>
        public int DefBreakerRetryTimeSec { get; set; }

        /// <summary>
        /// 熔断默认超时时间(秒)
        /// </summary>
        public int DefTimeOutBreakerSec { get; set; }
    }
}
