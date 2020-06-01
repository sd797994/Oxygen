using Autofac;
using Autofac.Extras.DynamicProxy;
using Dapr.Actors.AspNetCore;
using Dapr.Actors.Runtime;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oxygen.DaprActorProvider
{
    public class ActorServiceBuilder
    {
        private static readonly IOxygenLogger logger = OxygenIocContainer.Resolve<IOxygenLogger>();
        /// <summary>
        /// 注册actor服务到autofac
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorAutoModelSave(ContainerBuilder builder)
        {
            var remote = RpcInterfaceType.ActorTypes.Value;
            if (remote != null && remote.Any())
            {
                foreach (var type in remote)
                {
                    //给接口注册本地服务
                    if (type.classType != null)
                    {
                        if (type.autoSave)
                            builder.RegisterType(type.classType).As(type.interfaceType).SingleInstance().EnableInterfaceInterceptors().InterceptedBy(typeof(AutoSaveModelManager));
                        else
                            builder.RegisterType(type.classType).As(type.interfaceType).SingleInstance();
                    }
                }
            }
        }
        /// <summary>
        /// 注册actor中间件
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorMiddleware(object builder)
        {
            logger.LogInfo($"Actor服务开始注册");
            var webbuilder = (IWebHostBuilder)builder;
            webbuilder.UseActors(x => RegisterActor(x)).ConfigureServices(x => AddActorAutoMediatR(x));
        }
        /// <summary>
        /// 注册所有的actor服务
        /// </summary>
        /// <param name="runtime"></param>
        static void RegisterActor(ActorRuntime runtime)
        {
            var remote = RpcInterfaceType.ActorTypes.Value;
            if (remote != null && remote.Any())
            {
                foreach (var type in remote.Where(x => x.classType != null))
                {
                    typeof(ActorRuntime).GetMethod("RegisterActor").MakeGenericMethod(type.classType).Invoke(runtime, new object[] { default(Func<ActorTypeInformation, ActorService>) });
                    logger.LogInfo($"Actor服务[{type.classType.Name}]注册成功");
                }
            }
        }
        /// <summary>
        /// 注册MediatR用于actor服务自动保存
        /// </summary>
        /// <param name="services"></param>
        static void AddActorAutoMediatR(IServiceCollection services)
        {
            services.AddMediatR(typeof(ActorServiceBuilder).GetTypeInfo().Assembly);
        }
    }
}
