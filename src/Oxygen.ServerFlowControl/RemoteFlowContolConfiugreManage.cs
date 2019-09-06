using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using Oxygen.ICache;
using Oxygen.IServerFlowControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Loader;

namespace Oxygen.ServerFlowControl
{
    /// <summary>
    /// 流控配置管理器
    /// </summary>
    public class RemoteFlowContolConfiugreManage : IRemoteFlowContolConfiugreManage
    {
        private readonly ICacheService _cacheService;
        public RemoteFlowContolConfiugreManage(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }
        /// <summary>
        /// 读取本地更新断路配置
        /// </summary>
        public void SetCacheFromServices()
        {
            foreach (var type in GetTypes())
            {
                foreach(var method in type.InterFaceType.GetMethods())
                {
                    var attr = Attribute.GetCustomAttribute(method, typeof(FlowControlAttribute), false);
                    if (attr != null)
                    {
                        var flowControllerAttr = attr as FlowControlAttribute;
                        var serviceConfigInfo = Mapper<FlowControlAttribute, ServiceConfigureInfo>.Map(flowControllerAttr);
                        var cacheSetting = _cacheService.GetHashCache<ServiceConfigureInfo>(OxygenSetting.BreakerSettingKey, $"{type.ClassType.Name}{method.Name}");
                        if (cacheSetting == null)
                        {
                            _cacheService.SetHashCache(OxygenSetting.BreakerSettingKey, $"{type.ClassType.Name}{method.Name}", serviceConfigInfo);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取本地服务类型
        /// </summary>
        /// <returns></returns>
        private static List<TypeInfo> GetTypes()
        {
            var result = new List<TypeInfo>();
            var assemblys = GetAllAssemblies();
            var interfaceType = assemblys.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute)).Any() && t.IsInterface)).ToArray();
            foreach(var x in assemblys.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any() && interfaceType.Contains(t.GetInterfaces().FirstOrDefault()))))
            {
                result.Add(new TypeInfo(x, x.GetInterfaces().FirstOrDefault()));
            }
            return result;
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
    }
    class TypeInfo
    {
        public TypeInfo(Type classType, Type interfaceType)
        {
            InterFaceType = interfaceType;
            ClassType = classType;
        }
        public Type InterFaceType { get; set; }
        public Type ClassType { get; set; }
    }
}
