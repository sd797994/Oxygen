using Autofac;
using Microsoft.Extensions.DependencyModel;
using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 代理服务创建类
    /// </summary>
    public class ProxyClientBuilder
    {
        public static readonly Lazy<IRemoteProxyGenerator> ProxyGenerator = new Lazy<IRemoteProxyGenerator>(() => OxygenIocContainer.Resolve<IRemoteProxyGenerator>());
        public static ConcurrentDictionary<string, MethodDelegateInfo> Remotemethods = new ConcurrentDictionary<string, MethodDelegateInfo>();
        /// <summary>
        /// 为远程服务构建代理类
        /// </summary>
        /// <param name="builder"></param>
        public static void CreateRemoteProxyClientInstance(ContainerBuilder builder)
        {
            var remote = RpcInterfaceType.RemoteTypes.Value;
            if (remote != null && remote.Any())
            {
                foreach (var type in remote)
                {
                    builder.Register(x=>CreateTypeInstance(type)).As(type).InstancePerLifetimeScope().PropertiesAutowired();
                }
            }
        }

        public static object CreateTypeInstance(Type interfaceType)
        {
            var targetType = typeof(RemoteProxyDecoratorBuilder);
            return targetType.GetMethod("Create").MakeGenericMethod(interfaceType).Invoke(Activator.CreateInstance(targetType), null);
        }
    }
}
