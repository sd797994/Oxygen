using Microsoft.Extensions.DependencyModel;
using Oxygen.Common;
using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Oxygen.ProxyClientBuilder
{
    /// <summary>
    /// 代理服务创建类
    /// </summary>
    public class ProxyClientBuilder
    {
        private static Type[] GetTypes(bool isLocal)
        {
            var assemblys = GetAllAssemblies();
            var interfaceType = assemblys.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute)).Any() && t.IsInterface)).ToArray();
            if (isLocal)
            {
              return assemblys.SelectMany(x=>x.GetTypes().Where(t=> t.GetInterfaces().Any() && interfaceType.Contains(t.GetInterfaces().FirstOrDefault()))).ToArray();
            }
            else
            {
                return interfaceType.Except(assemblys.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any() && interfaceType.Contains(t.GetInterfaces().FirstOrDefault()))).Select(x=>x.GetInterfaces().FirstOrDefault()).ToArray()).ToArray();
            }
        }
        public static List<Assembly> GetAllAssemblies()
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
        /// <summary>
        /// 为客户端端创建远程代理服务类
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> CreateRemoteProxyClientBuilder()
        {
            var regType = GetTypes(false);
            if (regType.Any())
            {
                var bodys = RemoteProxyClientBuilder.GetBodyFromType(regType);
                return regType.BuildType("ProxyClient.g", bodys);
            }
            return default(IEnumerable<Type>);
        }

        /// <summary>
        /// 为服务端创建本地代理服务类
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> CreateLocalProxyClientBuilder()
        {
            var regType = GetTypes(true);
            if (regType.Any())
            {
                var bodys = LocalProxyClientBuilder.GetBodyFromType(regType);
                return regType.BuildType("LocalClient.g", bodys);
            }
            return default(IEnumerable<Type>);
        }
    }
}
