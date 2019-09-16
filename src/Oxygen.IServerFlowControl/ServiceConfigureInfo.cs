using Oxygen.CommonTool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Oxygen.IServerFlowControl
{
    /// <summary>
    /// 服务缓存配置
    /// </summary>
    public class ServiceConfigureInfo
    {
        /// <summary>
        /// 默认启用缓存
        /// </summary>
        public bool DefOpenCache { get; set; } = true;

        /// <summary>
        /// 默认缓存对象
        /// </summary>
        public object DefCacheData { get; set; } = null;

        /// <summary>
        /// 令牌桶默认最大容量(枚)
        /// </summary>
        public int DefCapacity { get; set; } = 100;

        /// <summary>
        /// 令牌生成桶默认间隔(秒)
        /// </summary>
        public int DefRateLimit { get; set; } = 1;
        /// <summary>
        /// 默认熔断阈值
        /// </summary>
        public int DefThresholdBreakeTimes { get; set; } = 10;

        /// <summary>
        /// 默认熔断阈值比率
        /// </summary>
        public double DefThresholdBreakeRatePerSec { get; set; } = 0.5F;
        /// <summary>
        /// 默认重试次数
        /// </summary>
        public int DefRetryTimes { get; set; } = 3;
        /// <summary>
        /// 默认单次请求重试时间(秒)
        /// </summary>
        public int DefRetryTimesSec { get; set; } = 5;

        /// <summary>
        /// 默认熔断后再次请求重试时间(秒)
        /// </summary>
        public int DefBreakerRetryTimeSec { get; set; } = 300;

        /// <summary>
        /// 熔断默认超时时间(秒)
        /// </summary>
        public int DefTimeOutBreakerSec { get; set; } = 3;

        /// <summary>
        /// 服务地址
        /// </summary>
        private List<FlowControlEndPoint> EndPoints { get; set; }


        /// <summary>
        /// 获取服务地址（深克隆）
        /// </summary>
        public List<FlowControlEndPoint> GetEndPoints()
        {
            return this.EndPoints ?? new List<FlowControlEndPoint>();
        }

        /// <summary>
        /// 设置服务地址
        /// </summary>
        public void SetEndPoints(List<FlowControlEndPoint> endPoints)
        {
            this.EndPoints = endPoints;
        }
        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="point"></param>
        /// <param name="data"></param>
        public void ReflushCache(object data)
        {
            if (DefOpenCache)
            {
                DefCacheData = data;
            }
        }
    }
}
