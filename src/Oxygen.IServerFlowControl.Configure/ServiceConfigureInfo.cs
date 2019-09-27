using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Oxygen.IServerFlowControl.Configure
{
    /// <summary>
    /// 服务缓存配置
    /// </summary>
    public class ServiceConfigureInfo
    {
        #region 属性
        /// <summary>
        /// 主键
        /// </summary>
        public string FlowControlCfgKey { get; set; }
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
        public int DefCapacity { get; set; } = 2000;

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
        /// 令牌桶更新时间
        /// </summary>
        public long StartTimeStamp { get; set; }

        /// <summary>
        /// 令牌桶个数
        /// </summary>
        public long Tokens { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        private List<FlowControlEndPoint> EndPoints { get; set; }
        #endregion
        #region 方法
        /// <summary>
        /// 获取服务地址
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
        /// 更新熔断结束的配置文件
        /// </summary>
        /// <param name="servcieInfo"></param>
        public void CleanBreakTimes()
        {
            List<FlowControlEndPoint> tmp = new List<FlowControlEndPoint>();
            GetEndPoints().ForEach(x =>
            {
                if ((x.BreakerTime != null && x.BreakerTime.Value.AddSeconds(DefBreakerRetryTimeSec) <= DateTime.Now))
                {
                    x.ThresholdBreakeTimes = 0;
                    x.BreakerTime = null;
                }
                tmp.Add(x);
            });
            SetEndPoints(tmp);
        }

        /// <summary>
        /// 删除配置节所有下属节点
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        public void RemoveAllNode()
        {
            SetEndPoints(null);
        }

        /// <summary>
        /// 根据服务路由更新配置节
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="addrs"></param>
        public void ReflushConfigureEndPoint(List<IPEndPoint> addrs)
        {
            //删除无效节点(即注册中心丢弃的非健康节点)
            var oldEndPoint = GetEndPoints().Select(x => x.GetEndPoint()).Except(addrs).ToList();
            SetEndPoints(GetEndPoints().Where(x => !oldEndPoint.Any(y => y.Equals(x.GetEndPoint()))).ToList());
            //增加新注册的节点
            var newEndPoint = addrs.Where(y => addrs.Except(GetEndPoints().Select(x => x.GetEndPoint())).Any(z => z.Equals(y)));
            SetEndPoints(GetEndPoints().Concat(newEndPoint.Select(x => new FlowControlEndPoint(x.Address, x.Port))).ToList());
        }


        /// <summary>
        /// 修改最小连接数
        /// </summary>
        public void ChangeConnectCount(IPEndPoint address, bool IsPlus)
        {
            var addr = GetEndPoints().FirstOrDefault(x => x.GetEndPoint().Equals(address));
            if (addr != null)
            {
                if (IsPlus)
                    addr.ConnectCount += 1;
                else
                    addr.ConnectCount = addr.ConnectCount <= 1 ? 0 : addr.ConnectCount - 1;
            }
        }
        /// <summary>
        /// 更新降级缓存
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public void ReflushCache(object entity)
        {
            if (DefOpenCache)
            {
                if (DefCacheData != null)
                {
                    DefCacheData = entity;
                }
            }
        }


        /// <summary>
        /// 更新令牌时间戳
        /// </summary>
        /// <param name="bucketInfo"></param>
        /// <param name="Capacity"></param>
        /// <param name="Rate"></param>
        public void UpdateTokens(long Capacity, long Rate)
        {
            var currentTime = DateTime.UtcNow.Ticks;
            if (currentTime < StartTimeStamp)
                return;
            Tokens = Capacity;
            StartTimeStamp = currentTime + Rate;
        }
        #endregion
    }
}