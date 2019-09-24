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
        public async Task UpdateBreakerConfigure(string flowControlCfgKey, ServiceConfigureInfo configure)
        {
            await _syncConfigureProvider.SetConfigure($"{OxygenSetting.BreakerSettingKey}{ flowControlCfgKey}", configure);
        }
        /// <summary>
        /// 服务端初始化配置节
        /// </summary>
        /// <param name="flowControlCfgKey"></param>
        /// <param name="servcieInfo"></param>
        /// <returns></returns>
        public async Task InitBreakerConfigure(string flowControlCfgKey, ServiceConfigureInfo configure)
        {
            await _syncConfigureProvider.InitConfigure($"{flowControlCfgKey}", configure);
        }
        /// <summary>
        /// 强制熔断无法连通的EndPoint
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="servcieInfo"></param>
        /// <param name="breakEndPoint"></param>
        public async Task ForcedCircuitBreakEndPoint(string flowControlCfgKey, ServiceConfigureInfo configure, IPEndPoint breakEndPoint)
        {
            var addr = configure.GetEndPoints().FirstOrDefault(x => x.GetEndPoint().Equals(breakEndPoint));
            if (addr != null)
            {
                addr.BreakerTime = DateTime.Now;
            }
            await UpdateBreakerConfigure(flowControlCfgKey, configure);
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
                        var flowControlCfgKey = $"{OxygenSetting.BreakerSettingKey}{type.ClassType.Name}{method.Name}";
                        if ((await GetBreakerConfigure(flowControlCfgKey)) == null)
                        {
                            await InitBreakerConfigure(flowControlCfgKey, serviceConfigInfo);
                        }
                    }
                }
            }
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
        public IPEndPoint GetServieByLoadBalane(List<FlowControlEndPoint> lbEndPoints, IPEndPoint clientIp, LoadBalanceType type = LoadBalanceType.IPHash, ServiceConfigureInfo configure = null)
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
            if (configure != null)
            {
                configure.ChangeConnectCount(result.GetEndPoint(), true);
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
