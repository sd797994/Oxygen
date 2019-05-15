using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Oxygen.Common.Logger;
using Oxygen.IServerProxyFactory;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 服务代理工厂类
    /// </summary>
    public class ServerProxyFactory : IServerProxyFactory.IServerProxyFactory
    {
        private readonly ILifetimeScope _container;
        private readonly IOxygenLogger _oxygenLogger;
        private static readonly ConcurrentDictionary<string, Type> InstanceDictionary = new ConcurrentDictionary<string, Type>();
        private static readonly Assembly Assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => x.FullName.Contains("ProxyClient.g"));
        public ServerProxyFactory(ILifetimeScope container, IOxygenLogger oxygenLogger)
        {
            _container = container;
            _oxygenLogger = oxygenLogger;
        }

        /// <summary>
        /// 通过强类型创建代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> CreateProxy<T>() where T : class
        {
            if (_container.TryResolve(typeof(T), out var instance))
            {
                return instance as T;
            }
            else
            {
                var className = $"{typeof(T).Name.Substring(1, typeof(T).Name.Length - 1)}_ProxyClient";
                var type = GetProxtClient(className);
                if (type != null)
                {
                    return Activator.CreateInstance(type) as T;
                }
                else
                {
                    _oxygenLogger.LogError($"未找到远程代理实例：{typeof(T).Name}");
                }
            }
            return await Task.FromResult(default(T));
        }

        /// <summary>
        /// 通过路径创建代理
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<IVirtualProxyServer> CreateProxy(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith("/api"))
                {
                    path = path.Replace("/api", "api");
                    if (_container.TryResolve(typeof(IVirtualProxyServer), out var instance))
                    {
                        var vitual = instance as VirtualProxyServer;
                        if (vitual != null)
                        {
                            var names = path.Split('/');
                            if (names.Length == 4)
                            {
                                if (names[0].Equals("api"))
                                {
                                    var className = $"{names[2]}_ProxyClient";
                                    var type = GetProxtClient(className);
                                    if (type != null)
                                    {
                                        vitual.ServerName = names[1];
                                        var pathName = $"{names[2]}_{type.GetMethod(names[3])?.GetParameters().FirstOrDefault()?.ParameterType.Name}";
                                        vitual.PathName = pathName;
                                    }
                                }
                            }
                        }
                        return vitual;
                    }
                }
            }
            return await Task.FromResult(default(IVirtualProxyServer));
        }

        /// <summary>
        /// 通过代理程序集获取类型
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        Type GetProxtClient(string className)
        {

            if (!InstanceDictionary.TryGetValue(className, out var messageType))
            {
                messageType = Assembly.GetType($"Oxygen.RemoteProxyClientBuilder.ProxyInstance.{className}");
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
