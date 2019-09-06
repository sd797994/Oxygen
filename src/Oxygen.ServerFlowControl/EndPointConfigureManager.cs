using Oxygen.CommonTool;
using Oxygen.ICache;
using Oxygen.IServerFlowControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Oxygen.ServerFlowControl
{
    public class EndPointConfigureManager: IEndPointConfigureManager
    {
        private readonly ICacheService _cacheService;
        public EndPointConfigureManager(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }
        /// <summary>
        /// 获取服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public ServiceConfigureInfo GetOrAddBreakerConfigure(string pathName)
        {
            var servcieInfo = _cacheService.GetHashCache<ServiceConfigureInfo>(OxygenSetting.BreakerSettingKey, pathName);
            if (servcieInfo == null)
            {
                return null;
            }
            servcieInfo.EndPoints = servcieInfo.EndPoints ?? new List<FlowControlEndPoint>();
            return servcieInfo;
        }
        /// <summary>
        /// 更新服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public void UpdateBreakerConfigure(string pathName, ServiceConfigureInfo servcieInfo)
        {
            _cacheService.SetHashCache(OxygenSetting.BreakerSettingKey, pathName, servcieInfo);
        }
        /// <summary>
        /// 更新熔断结束的配置文件
        /// </summary>
        /// <param name="servcieInfo"></param>
        public void CleanBreakTimes(ServiceConfigureInfo servcieInfo)
        {
            servcieInfo.EndPoints.ForEach(x =>
            {
                if ((x.BreakerTime != null && x.BreakerTime.Value.AddSeconds(servcieInfo.DefBreakerRetryTimeSec) <= DateTime.Now))
                {
                    x.ThresholdBreakeTimes = 0;
                }
                x.BreakerTime = null;
            });
        }

        /// <summary>
        /// 根据服务路由更新配置节
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="addrs"></param>
        public void ReflushConfigureEndPoint(ServiceConfigureInfo serviceInfo, List<FlowControlEndPoint> addrs)
        {
            //删除无效节点(即注册中心丢弃的非健康节点)
            var oldEndPoint = serviceInfo.EndPoints.Select(x => x.GetEndPoint()).Except(addrs.Select(x => x.GetEndPoint())).ToList();
            serviceInfo.EndPoints = serviceInfo.EndPoints.Where(x => !oldEndPoint.Any(y => y.Equals(x.GetEndPoint()))).ToList();
            //增加新注册的节点
            var newEndPoint = addrs.Where(y => addrs.Select(x => x.GetEndPoint()).Except(serviceInfo.EndPoints.Select(x => x.GetEndPoint())).Any(z => z.Equals(y.GetEndPoint())));
            serviceInfo.EndPoints.AddRange(newEndPoint.Select(x => new FlowControlEndPoint(x.Address, x.Port)));
        }

        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        static IPEndPoint TargetIp;
        static int TargetIpSortInex;
        public IPEndPoint GetServieByLoadBalane(List<FlowControlEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash)
        {
            var result = default(FlowControlEndPoint);
            if (lbEndPoints != null && lbEndPoints.Any())
            {
                //若没有客户端IP则默认调用随机
                if (clientIp == null)
                    type = LoadBalanceType.Random;
                switch (type)
                {
                    //随机
                    case LoadBalanceType.Random:
                        result = lbEndPoints.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                        break;
                    //轮询
                    case LoadBalanceType.Polling:
                        result = TargetIp == null ? lbEndPoints.FirstOrDefault() :
                        lbEndPoints.Any(x => x.HashSort > TargetIpSortInex) ?
                            lbEndPoints.First(x => x.HashSort > TargetIpSortInex) :
                            lbEndPoints.First();
                        TargetIp = result.GetEndPoint();
                        TargetIpSortInex = result.HashSort;
                        break;
                    //IP哈希
                    case LoadBalanceType.IPHash:
                        result = lbEndPoints[Math.Abs(clientIp.GetHashCode()) % lbEndPoints.Count];
                        break;
                    //最小连接
                    case LoadBalanceType.MinConnections:
                        result = lbEndPoints.OrderBy(x => x.ConnectCount).FirstOrDefault();
                        break;
                }
            }
            if (result != default(FlowControlEndPoint))
            {
                ChangeConnectCount(lbEndPoints, result.GetEndPoint(), true);
            }
            return result.GetEndPoint();
        }

        /// <summary>
        /// 修改最小连接数
        /// </summary>
        public void ChangeConnectCount(List<FlowControlEndPoint> lbEndPoints, IPEndPoint address, bool IsPlus)
        {
            var addr = lbEndPoints.FirstOrDefault(x => x.GetEndPoint().Equals(address));
            if (addr != null)
            {
                if (IsPlus)
                    addr.ConnectCount += 1;
                else
                    addr.ConnectCount = addr.ConnectCount <= 1 ? 0 : addr.ConnectCount - 1;
            }
        }
    }
}
