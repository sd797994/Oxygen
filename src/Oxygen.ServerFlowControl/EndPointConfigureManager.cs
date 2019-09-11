using Microsoft.Extensions.DependencyModel;
using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using Oxygen.ICache;
using Oxygen.IServerFlowControl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Oxygen.ServerFlowControl
{
    /// <summary>
    /// 流控配置管理器
    /// </summary>
    public class EndPointConfigureManager: IEndPointConfigureManager
    {
        private readonly ICacheService _cacheService;
        private static Lazy<ConcurrentDictionary<string, ServiceConfigureInfo>> _localBreakSetting = new Lazy<ConcurrentDictionary<string, ServiceConfigureInfo>>(() => { return new ConcurrentDictionary<string, ServiceConfigureInfo>(); });
        private static Lazy<ConcurrentDictionary<string, TokenBucketInfo>> _localLimitSetting = new Lazy<ConcurrentDictionary<string, TokenBucketInfo>>(() => { return new ConcurrentDictionary<string, TokenBucketInfo>(); });

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
            if (_localBreakSetting.Value.TryGetValue($"{OxygenSetting.BreakerSettingKey}{pathName}",out ServiceConfigureInfo servcieInfo))
            {
                return servcieInfo;
            }
            else
            {
                servcieInfo = _cacheService.GetHashCache<ServiceConfigureInfo>(OxygenSetting.BreakerSettingKey, pathName);
                if (servcieInfo != null)
                {
                    servcieInfo.EndPoints = servcieInfo.EndPoints ?? new List<FlowControlEndPoint>();
                }
                _localBreakSetting.Value.TryAdd($"{OxygenSetting.BreakerSettingKey}{pathName}", servcieInfo);
            }
            return servcieInfo;
        }
        /// <summary>
        /// 更新服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public void UpdateBreakerConfigure(string pathName, ServiceConfigureInfo servcieInfo)
        {
            //更新本地缓存
            _localBreakSetting.Value.TryRemove($"{OxygenSetting.BreakerSettingKey}{pathName}", out _);
            _localBreakSetting.Value.TryAdd($"{OxygenSetting.BreakerSettingKey}{pathName}", servcieInfo);
            //将变更发布到订阅端进行更新
            _cacheService.PublishAsync($"{OxygenSetting.BreakerSettingKey}{pathName}", servcieInfo);
        }
        /// <summary>
        /// 强制熔断无法连通的EndPoint
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        /// <param name="breakEndPoint"></param>
        public void ForcedCircuitBreakEndPoint(string pathName, ServiceConfigureInfo servcieInfo, IPEndPoint breakEndPoint)
        {
            var addr = servcieInfo.EndPoints.FirstOrDefault(x => x.GetEndPoint().Equals(breakEndPoint));
            if (addr != null)
            {
                addr.BreakerTime = DateTime.Now;
                UpdateBreakerConfigure(pathName, servcieInfo);
            }
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
                    x.BreakerTime = null;
                }
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
                if (clientIp == null && type == LoadBalanceType.IPHash)
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

        /// <summary>
        /// 读取本地更新断路配置
        /// </summary>
        public void SetCacheFromServices()
        {
            foreach (var type in GetLocalTypes())
            {
                foreach (var method in type.InterFaceType.GetMethods())
                {
                    var attr = Attribute.GetCustomAttribute(method, typeof(FlowControlAttribute), false);
                    if (attr != null)
                    {
                        var flowControllerAttr = attr as FlowControlAttribute;
                        var serviceConfigInfo = Mapper<FlowControlAttribute, ServiceConfigureInfo>.Map(flowControllerAttr);
                        var cacheSetting = _cacheService.GetHashCache<ServiceConfigureInfo>(OxygenSetting.BreakerSettingKey, $"{type.ClassType.Name}{method.Name}");
                        if (cacheSetting == null)
                        {
                            //写入本地缓存并更新配置中心数据
                            _localBreakSetting.Value.TryAdd($"{OxygenSetting.BreakerSettingKey}{type.ClassType.Name}{method.Name}", serviceConfigInfo);
                            _cacheService.SetHashCache(OxygenSetting.BreakerSettingKey, $"{type.ClassType.Name}{method.Name}", serviceConfigInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 客户端注册熔断配置节缓存订阅
        /// </summary>
        public void SubscribeAllService()
        {
            var _eventWait = new AutoResetEvent(false);
            foreach (var type in GetRemoteInterfaceTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    //订阅熔断配置
                    var topicKey = $"{OxygenSetting.BreakerSettingKey}{type.Name.Substring(1, type.Name.Length - 1)}{method.Name}";
                    _cacheService.SubscribeAsync<ServiceConfigureInfo>(topicKey, (serviceConfigInfo) =>
                    {
                        _localBreakSetting.Value.TryRemove(topicKey, out _);
                        _localBreakSetting.Value.TryAdd(topicKey, serviceConfigInfo);
                        _eventWait.Set();
                    });
                    //订阅限流配置
                    topicKey = $"{OxygenSetting.TokenLimitSettingKey}{type.Name.Substring(1, type.Name.Length - 1)}{method.Name}";
                    _cacheService.SubscribeAsync<TokenBucketInfo>(topicKey, (bucketInfo) =>
                    {
                        _localLimitSetting.Value.TryRemove(topicKey, out _);
                        _localLimitSetting.Value.TryAdd(topicKey, bucketInfo);
                        _eventWait.Set();
                    });
                }
            }
            while (true)
            {
                _eventWait.WaitOne();
            }
        }
        /// <summary>
        /// 获取限流令牌桶配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        public TokenBucketInfo GetOrAddTokenBucket(string key, ServiceConfigureInfo serviceInfo)
        {
            if (_localLimitSetting.Value.TryGetValue($"{OxygenSetting.TokenLimitSettingKey}{key}", out TokenBucketInfo bucketInfo))
            {
                return bucketInfo;
            }
            else
            {
                bucketInfo = _cacheService.GetHashCache<TokenBucketInfo>(OxygenSetting.TokenLimitSettingKey, key);
                if (bucketInfo == null)
                {
                    bucketInfo = new TokenBucketInfo();
                    bucketInfo.Tokens = serviceInfo.DefCapacity;
                    bucketInfo.StartTimeStamp = DateTime.UtcNow.Ticks;
                    _cacheService.SetHashCache(OxygenSetting.TokenLimitSettingKey, key, bucketInfo);
                }
                _localLimitSetting.Value.TryAdd($"{OxygenSetting.TokenLimitSettingKey}{key}", bucketInfo);
            }
            return bucketInfo;
        }

        /// <summary>
        /// 更新令牌时间戳
        /// </summary>
        /// <param name="bucketInfo"></param>
        /// <param name="Capacity"></param>
        /// <param name="Rate"></param>
        public void UpdateTokens(TokenBucketInfo bucketInfo, long Capacity, long Rate)
        {
            var currentTime = DateTime.UtcNow.Ticks;
            if (currentTime < bucketInfo.StartTimeStamp)
                return;
            bucketInfo.Tokens = Capacity;
            bucketInfo.StartTimeStamp = currentTime + Rate;
        }

        /// <summary>
        /// 更新令牌数量并发布
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="bucketInfo"></param>
        public void UpdateTokenBucket(string key, TokenBucketInfo bucketInfo)
        {
            //更新本地缓存
            _localLimitSetting.Value.TryRemove($"{OxygenSetting.TokenLimitSettingKey}{key}", out _);
            _localLimitSetting.Value.TryAdd($"{OxygenSetting.TokenLimitSettingKey}{key}", bucketInfo);
            //将变更发布到订阅端进行更新
            _cacheService.PublishAsync($"{OxygenSetting.TokenLimitSettingKey}{key}", bucketInfo);
        }

        #region 私有方法
        /// <summary>
        /// 获取本地服务类型
        /// </summary>
        /// <returns></returns>
        private static List<FlowContolConfiugreTypeInfo> GetLocalTypes()
        {
            var result = new List<FlowContolConfiugreTypeInfo>();
            var assemblys = GetAllAssemblies();
            var interfaceType = assemblys.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute)).Any() && t.IsInterface)).ToArray();
            foreach (var x in assemblys.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any() && interfaceType.Contains(t.GetInterfaces().FirstOrDefault()))))
            {
                result.Add(new FlowContolConfiugreTypeInfo(x, x.GetInterfaces().FirstOrDefault()));
            }
            return result;
        }
        /// <summary>
        /// 获取客户端服务类型
        /// </summary>
        /// <returns></returns>
        private static List<Type> GetRemoteInterfaceTypes()
        {
            var result = new List<FlowContolConfiugreTypeInfo>();
            var assemblys = GetAllAssemblies();
            var interfaceType = assemblys.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute)).Any() && t.IsInterface)).ToArray();
            return interfaceType.Except(assemblys.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any() && interfaceType.Contains(t.GetInterfaces().FirstOrDefault()))).Select(x => x.GetInterfaces().FirstOrDefault()).ToArray()).ToList();
        }
        /// <summary>
        /// 获取当前程序集
        /// </summary>
        /// <returns></returns>
        private static List<Assembly> GetAllAssemblies()
        {
            var list = new List<Assembly>();
            var deps = DependencyContext.Default;
            var libs = deps.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type != "package");//排除所有的系统程序集、Nuget下载包
            foreach (var lib in libs)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
                    list.Add(assembly);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return list;
        }
        #endregion
    }
}
