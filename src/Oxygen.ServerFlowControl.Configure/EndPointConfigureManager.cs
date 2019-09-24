using Microsoft.Extensions.DependencyModel;
using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using Oxygen.ICache;
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.ServerFlowControl.Configure
{
    /// <summary>
    /// 流控配置管理器
    /// </summary>
    public class EndPointConfigureManager: IEndPointConfigureManager
    {
        private readonly ISyncConfigureProvider _syncConfigureProvider;
        public EndPointConfigureManager(ISyncConfigureProvider syncConfigureProvider)
        {
            _syncConfigureProvider = syncConfigureProvider;
        }
        #region ServiceConfigureInfo
        public async Task<bool> CheckBreakerConfigureAny(string flowControlCfgKey)
        {
            return await GetBreakerConfigure(flowControlCfgKey) != null;
        }
        /// <summary>
        /// 获取服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public async Task<ServiceConfigureInfo> GetBreakerConfigure(string flowControlCfgKey)
        {
            return await _syncConfigureProvider.GetConfigure($"{OxygenSetting.BreakerSettingKey}{ flowControlCfgKey}");
        }
        /// <summary>
        /// 更新服务配置节
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public async Task UpdateBreakerConfigure(string flowControlCfgKey, ServiceConfigureInfo servcieInfo)
        {
            await _syncConfigureProvider.SetConfigure($"{OxygenSetting.BreakerSettingKey}{ flowControlCfgKey}", servcieInfo);
        }
        /// <summary>
        /// 服务端初始化配置节
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="servcieInfo"></param>
        /// <returns></returns>
        public async Task InitBreakerConfigure(string flowControlCfgKey, ServiceConfigureInfo servcieInfo)
        {
            await _syncConfigureProvider.InitConfigure($"{OxygenSetting.BreakerSettingKey}{ flowControlCfgKey}", servcieInfo);
        }
        /// <summary>
        /// 强制熔断无法连通的EndPoint
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        /// <param name="breakEndPoint"></param>
        public async Task ForcedCircuitBreakEndPoint(string flowControlCfgKey, IPEndPoint breakEndPoint)
        {
            var config =await GetBreakerConfigure(flowControlCfgKey);
            var addr = config.GetEndPoints().FirstOrDefault(x => x.GetEndPoint().Equals(breakEndPoint));
            if (addr != null)
            {
                addr.BreakerTime = DateTime.Now;
                await UpdateBreakerConfigure(flowControlCfgKey, config);
            }
        }
        /// <summary>
        /// 更新熔断结束的配置文件
        /// </summary>
        /// <param name="servcieInfo"></param>
        public async Task CleanBreakTimes(string flowControlCfgKey)
        {
            var config = await GetBreakerConfigure(flowControlCfgKey);
            List<FlowControlEndPoint> tmp = new List<FlowControlEndPoint>();
            config.GetEndPoints().ForEach(x =>
            {
                if ((x.BreakerTime != null && x.BreakerTime.Value.AddSeconds(config.DefBreakerRetryTimeSec) <= DateTime.Now))
                {
                    x.ThresholdBreakeTimes = 0;
                    x.BreakerTime = null;
                }
                tmp.Add(x);
            });
            config.SetEndPoints(tmp);
            await UpdateBreakerConfigure(flowControlCfgKey, config);
        }

        /// <summary>
        /// 删除配置节所有下属节点
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        public async Task RemoveAllNode(string flowControlCfgKey)
        {
            var config = await GetBreakerConfigure(flowControlCfgKey);
            config.SetEndPoints(null);
            await UpdateBreakerConfigure(flowControlCfgKey, config);
        }

        /// <summary>
        /// 根据服务路由更新配置节
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <param name="addrs"></param>
        public async Task ReflushConfigureEndPoint(string flowControlCfgKey, List<IPEndPoint> addrs)
        {
            var config = await GetBreakerConfigure(flowControlCfgKey);
            //删除无效节点(即注册中心丢弃的非健康节点)
            var oldEndPoint = config.GetEndPoints().Select(x => x.GetEndPoint()).Except(addrs).ToList();
            config.SetEndPoints(config.GetEndPoints().Where(x => !oldEndPoint.Any(y => y.Equals(x.GetEndPoint()))).ToList());
            //增加新注册的节点
            var newEndPoint = addrs.Where(y => addrs.Except(config.GetEndPoints().Select(x => x.GetEndPoint())).Any(z => z.Equals(y)));
            config.SetEndPoints(config.GetEndPoints().Concat(newEndPoint.Select(x => new FlowControlEndPoint(x.Address, x.Port))).ToList());
            await UpdateBreakerConfigure(flowControlCfgKey, config);
        }


        /// <summary>
        /// 修改最小连接数
        /// </summary>
        public async Task ChangeConnectCount(string flowControlCfgKey, IPEndPoint address, bool IsPlus)
        {
            var config = await GetBreakerConfigure(flowControlCfgKey);
            var addr = config.GetEndPoints().FirstOrDefault(x => x.GetEndPoint().Equals(address));
            if (addr != null)
            {
                if (IsPlus)
                    addr.ConnectCount += 1;
                else
                    addr.ConnectCount = addr.ConnectCount <= 1 ? 0 : addr.ConnectCount - 1;
                await UpdateBreakerConfigure(flowControlCfgKey, config);
            }
        }
        /// <summary>
        /// 更新降级缓存
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task ReflushCache(string flowControlCfgKey, object entity)
        {
            var config = await GetBreakerConfigure(flowControlCfgKey);
            if (config.DefOpenCache)
            {
                if (config.DefCacheData != null)
                {
                    config.DefCacheData = entity;
                    await UpdateBreakerConfigure(flowControlCfgKey, config);
                }
            }
        }

        /// <summary>
        /// 读取本地更新断路配置
        /// </summary>
        public async Task SetCacheFromServices()
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
                        if (!(await CheckBreakerConfigureAny($"{OxygenSetting.BreakerSettingKey}{type.ClassType.Name}{method.Name}")))
                        {
                            await InitBreakerConfigure($"{OxygenSetting.BreakerSettingKey}{type.ClassType.Name}{method.Name}", serviceConfigInfo);
                        }
                    }
                }
            }
        }
        #endregion
        #region TokenBucketInfo

        /// <summary>
        /// 获取限流令牌桶配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        public async Task<TokenBucketInfo> GetOrAddTokenBucket(string flowControlCfgKey, int defCapacity)
        {
            var bucket =await _syncConfigureProvider.GetBucket($"{OxygenSetting.TokenLimitSettingKey}{flowControlCfgKey}");
            if (bucket!=null)
            {
                return bucket;
            }
            else
            {
                bucket = new TokenBucketInfo
                {
                    Tokens = defCapacity,
                    StartTimeStamp = DateTime.UtcNow.Ticks
                };
                await _syncConfigureProvider.SetBucket($"{OxygenSetting.TokenLimitSettingKey}{flowControlCfgKey}", bucket);
            }
            return bucket;
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
        public async Task UpdateTokenBucket(string key, TokenBucketInfo bucketInfo)
        {
            await _syncConfigureProvider.SetBucket($"{OxygenSetting.TokenLimitSettingKey}{key}", bucketInfo);
        }
        #endregion
        #region 负载均衡
        /// <summary>
        /// 通过负载均衡返回一个ip地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        static IPEndPoint TargetIp;
        static int TargetIpSortInex;
        public IPEndPoint GetServieByLoadBalane(List<IPEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash)
        {
            return GetServieByLoadBalane(lbEndPoints.GetFlowControlEndPoints(), clientIp, type);
        }
        public IPEndPoint GetServieByLoadBalane(List<FlowControlEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash, string flowControlCfgKey = null)
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
            if (!string.IsNullOrEmpty(flowControlCfgKey))
            {
                ChangeConnectCount(flowControlCfgKey, result.GetEndPoint(), true);
            }
            return result.GetEndPoint();
        }
        #endregion
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
