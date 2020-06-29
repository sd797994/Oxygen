using Autofac;
using Dapr.Actors;
using Dapr.Actors.AspNetCore;
using Dapr.Actors.Runtime;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.DaprActorProvider.StateManage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    public class ActorServiceBuilder
    {
        /// <summary>
        /// 注册actor到容器并提供中介者用于自动保存
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorToContainer(ContainerBuilder builder)
        {
            RpcInterfaceType.ActorTypes.Value.Where(x => x.classType != null).ToList().ForEach(x => builder.RegisterType(x.classType).As(x.interfaceType).InstancePerDependency());
            builder.RegisterType<Mediator>().As<IMediator>().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(ActorStateSubscriber).GetTypeInfo().Assembly).AsClosedTypesOf(typeof(INotificationHandler<>)).AsImplementedInterfaces();
            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
        /// <summary>
        /// 通过actorruntime 注册actor
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorMiddleware(object builder, ILifetimeScope container)
        {
            ((IWebHostBuilder)builder).UseActors(x => RegisterActor(x, container));
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
                    Func<ActorTypeInformation, ActorService> createFunc = (info) => new ActorService(info, (actorService, actorId) =>
                    {
                        var actorInstance = container.Resolve(type.interfaceType, new TypedParameter(typeof(ActorService), actorService), new TypedParameter(typeof(ActorId), actorId), new TypedParameter(typeof(ILifetimeScope), container)) as OxygenActorBase;
                        actorInstance.AutoSave = type.autoSave;
                        return actorInstance;
                    });
                    typeof(ActorRuntime).GetMethod("RegisterActor").MakeGenericMethod(type.classType).Invoke(runtime, new object[] { createFunc });
                }
            }
        }
    }
}