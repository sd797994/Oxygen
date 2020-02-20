using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.CsharpClientAgent;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oxygen.ServerProxyFactory
{
    public class RemoteProxyDecoratorBuilder
    {
        private static readonly Lazy<IOxygenLogger> logger = new Lazy<IOxygenLogger>(() => OxygenIocContainer.Resolve<IOxygenLogger>());
        public static ConcurrentDictionary<string, MethodDelegateInfo> Remotemethods = new ConcurrentDictionary<string, MethodDelegateInfo>();
        public static Lazy<IRemoteProxyGenerator> ProxyGenerator = new Lazy<IRemoteProxyGenerator>(OxygenIocContainer.Resolve<IRemoteProxyGenerator>());
        public T CreateProxyInstance<T>()
        {
            return DispatchProxy.Create<T, RemoteProxyDecorator<T>>();
        }
        public static void RegisterProxyInDic(Type type)
        {
            var serviceName = (string)typeof(RemoteServiceAttribute).GetProperty("ServerName")
                       ?.GetValue(type.GetCustomAttribute(typeof(RemoteServiceAttribute)));
            foreach (var method in type.GetMethods())
            {
                var key = method.Name + string.Join("", method.GetParameters().Select(x => x.Name));
                var tmpmod = new MethodDelegateInfo()
                {
                    ServiceName = serviceName,
                    PathName = $"{type.Name}/{method.Name}",
                    MethodInfo = typeof(IRemoteProxyGenerator).GetMethod("SendAsync").MakeGenericMethod(method.GetParameters()[0].ParameterType, method.ReturnParameter.ParameterType.GenericTypeArguments[0])
                };
                if (!Remotemethods.TryAdd($"{tmpmod.PathName}", tmpmod))
                {
                    logger.Value.LogError($"无法为远程代理添加同名服务{tmpmod.PathName},请确保服务名全局唯一");
                }
            }
        }
    }
}
