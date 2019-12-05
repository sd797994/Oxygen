using Autofac;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.CsharpClientAgent;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 服务代理工厂类
    /// </summary>
    public class ServerProxyFactory : IServerProxyFactory.IServerProxyFactory
    {
        private readonly ILifetimeScope _container;
        private static readonly ConcurrentDictionary<string, Type> InstanceDictionary = new ConcurrentDictionary<string, Type>();
        private static readonly ConcurrentDictionary<string, string[]> InstanceParmDictionary = new ConcurrentDictionary<string, string[]>();
        public ServerProxyFactory(ILifetimeScope container)
        {
            _container = container;
        }

        /// <summary>
        /// 通过强类型创建代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateProxy<T>() where T : class
        {
            if (_container.TryResolve(typeof(T), out var instance))
            {
                return instance as T;
            }
            return default(T);
        }

        /// <summary>
        /// 通过路径创建代理
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IVirtualProxyServer CreateProxy(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (path.ToLower().StartsWith("/api"))
                {
                    path = path.Replace("/api", "api");
                    if (_container.TryResolve(typeof(IVirtualProxyServer), out var instance))
                    {
                        var vitual = instance as VirtualProxyServer;
                        if (vitual != null)
                        {
                            if (InstanceParmDictionary.TryGetValue(path.ToLower(), out var messageType))
                            {
                                vitual.Init(messageType[0], messageType[1]);
                            }
                            else
                            {
                                var names = path.Split('/');
                                if (names.Length == 4)
                                {
                                    if (names[0].ToLower().Equals("api"))
                                    {
                                        var className = $"{names[2]}";
                                        var type = GetProxyClient(className);
                                        if (type != null)
                                        {
                                            var serviceName = (string)typeof(RemoteServiceAttribute).GetProperty("ServerName")
                                                    ?.GetValue(type.GetCustomAttribute(typeof(RemoteServiceAttribute)));
                                            var method = type.GetMethods().FirstOrDefault(x => x.Name.ToLower().Equals(names[3].ToLower()));
                                            if (method != null)
                                            {
                                                InstanceParmDictionary.TryAdd(path.ToLower(),new[] { serviceName, $"{type.Name}/{method.Name}" });
                                                vitual.Init(serviceName, $"{type.Name}/{method.Name}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return vitual;
                    }
                }
            }
            return default(IVirtualProxyServer);
        }

        /// <summary>
        /// 通过代理程序集获取类型
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        Type GetProxyClient(string className)
        {
            if (!InstanceDictionary.TryGetValue(className, out var messageType))
            {
                messageType = RpcInterfaceType.Types.Value.FirstOrDefault(x => x.Name.ToLower().Contains(className.ToLower()));
                if (messageType != null)
                {
                    InstanceDictionary.TryAdd(className, messageType);
                }
            }
            if (messageType != null)
            {
                return messageType;
            }
            return null;
        }
    }
}
