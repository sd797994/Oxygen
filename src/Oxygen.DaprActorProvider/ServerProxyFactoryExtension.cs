using Dapr.Actors;
using Dapr.Actors.Client;
using System;
using System.Collections.Concurrent;

namespace Oxygen.DaprActorProvider
{
    public static class ServerProxyFactoryExtension
    {
        static Lazy<ConcurrentDictionary<string, IActor>> ActorDir = new Lazy<ConcurrentDictionary<string, IActor>>(() => new ConcurrentDictionary<string, IActor>());
        /// <summary>
        /// 通过强类型创建代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateProxy<T>(this IServerProxyFactory.IServerProxyFactory factory, object key) where T : IActor
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
    }
}