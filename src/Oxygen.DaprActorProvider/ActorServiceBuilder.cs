using Autofac;
using Dapr.Actors;
using Dapr.Actors.AspNetCore;
using Dapr.Actors.Runtime;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.DaprActorProvider.Aspect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    public class ActorServiceBuilder
    {
        static Dictionary<string,Type> proxyTypes = new Dictionary<string, Type>();

        /// <summary>
        /// 创建代理服务类型用于稍后注册actor代理
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorToContainer()
        {
            RpcInterfaceType.ActorTypes.Value?.Where(x => x.classType != null).ToList().ForEach(type => proxyTypes.TryAdd(type.interfaceType.FullName, new ProxyActorBuilder(type.interfaceType, type.classType).CreateType()));
        }
        /// <summary>
        /// 注册代理的中介者处理程序
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterProxyMediatR(ContainerBuilder builder)
        {
            builder.RegisterType<Mediator>().As<IMediator>().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(ActorStateSubscriber).GetTypeInfo().Assembly).AsClosedTypesOf(typeof(INotificationHandler<>)).AsImplementedInterfaces();
            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
        /// <summary>
        /// 注册actor中间件
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorMiddleware(object builder, ILifetimeScope container)
        {
            var webbuilder = (IWebHostBuilder)builder;
            webbuilder.UseActors(x => RegisterActor(x, container));
        }
        /// <summary>
        /// 注册所有的actor服务
        /// </summary>
        /// <param name="runtime"></param>
        static void RegisterActor(ActorRuntime runtime, ILifetimeScope container)
        {
            var remote = RpcInterfaceType.ActorTypes.Value;
            if (remote != null && remote.Any())
            {
                foreach (var type in remote.Where(x => x.classType != null))
                {
                    var proxyType = proxyTypes.FirstOrDefault(x => x.Key.Equals(type.interfaceType.FullName)).Value;
                    Func<ActorTypeInformation, ActorService> createFunc = (info) => new ActorService(info, (actorService, actorId) =>
                    {
                        return Activator.CreateInstance(proxyType, new object[] { actorService, actorId, container }) as Actor;
                    });
                    typeof(ActorRuntime).GetMethod("RegisterActor").MakeGenericMethod(proxyType).Invoke(runtime, new object[] { createFunc });
                }
            }
        }
    }
}