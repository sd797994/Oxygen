using Dapr.Actors;
using Dapr.Actors.Client;
using Oxygen.CommonTool;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    public static class ServerProxyFactoryExtension
    {
        static Lazy<ConcurrentDictionary<string, IActor>> ActorDir = new Lazy<ConcurrentDictionary<string, IActor>>(() => new ConcurrentDictionary<string, IActor>());
        static Lazy<ConcurrentDictionary<string, VirtualActorProxyServer>> ActorProxyDir = new Lazy<ConcurrentDictionary<string, VirtualActorProxyServer>>(() => new ConcurrentDictionary<string, VirtualActorProxyServer>());
        /// <summary>
        /// 通过强类型创建代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateProxy<T>(this IServerProxyFactory.IServerProxyFactory factory, object key = null) where T : IActor
        {
            var name = typeof(T).Name[1..];
            if (ActorDir.Value.TryGetValue($"{name}{key}", out IActor instance))
            {
                return (T)instance;
            }
            else
            {
                var actorProxy = ActorProxy.Create<T>(new ActorId(key.ToString()), name);
                ActorDir.Value.TryAdd($"{name}{key}", actorProxy);
                return actorProxy;
            }
        }
        public static IVirtualProxyServer CreateProxy(this IServerProxyFactory.IServerProxyFactory factory, string path, object key = null)
        {
            if (key == null)
                return factory.CreateProxy(path);
            if (path.ToLower().StartsWith("/api"))
            {
                path = path.Replace("/api", "api");
                if (ActorProxyDir.Value.TryGetValue($"{path.ToLower()}", out VirtualActorProxyServer instance))
                {
                    return instance;
                }
                else
                {
                    var names = path.Split('/');
                    if (names.Length == 4)
                    {
                        if (names[0].ToLower().Equals("api"))
                        {
                            var ActorType = names[2];
                            var Method = names[3];
                            var actorProxy = new VirtualActorProxyServer(Method, ActorType, key.ToString());
                            ActorProxyDir.Value.TryAdd($"{path.ToLower()}", actorProxy);
                            return actorProxy;
                        }
                    }
                }
            }
            return default;
        }
    }
}