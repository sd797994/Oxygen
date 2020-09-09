﻿using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oxygen.CommonTool;
using Oxygen.IRpcProviderService;
using Oxygen.ServerProxyFactory;
using System;
using System.Threading.Tasks;

namespace Oxygen
{
    /// <summary>
    /// 配置中心
    /// </summary>
    public static class RpcConfigurationModule
    {
        private static bool CONFIGSERVICE = false;
        /// <summary>
        /// 依赖注入Oxygen服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContainerBuilder RegisterOxygen(this ContainerBuilder builder)
        {
            //注入rpc服务
            switch (OxygenSetting.MeshType)
            {
                case EnumMeshType.None:
                case EnumMeshType.Istio:
                    switch (OxygenSetting.ProtocolType)
                    {
                        case EnumProtocolType.TCP:
                        case EnumProtocolType.HTTP11:
                        default:
                            builder.RegisterModule(new DotNettyRpcProviderService.Module());
                            break;
                        case EnumProtocolType.HTTP2:
                            builder.RegisterModule(new KestrelRpcProviderService.Module());
                            break;
                    }
                    break;
                case EnumMeshType.Dapr:
                    builder.RegisterModule(new KestrelRpcProviderService.Module());//dapr目前仅支持kestrel
                    builder.RegisterModule(new DaprActorProvider.Module());//加载dapr actor
                    break;
            }
            //注入序列化服务
            builder.RegisterModule(new MessagePackSerializeService.Module());
            //注入代理工厂
            builder.RegisterModule(new ServerProxyFactory.Module());
            //注入通用服务
            builder.RegisterModule(new CommonTool.Module());
            return builder;
        }
        /// <summary>
        /// 注册成为Oxygen服务节点
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IHostBuilder UseOxygenService(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection> collection)
        {
            CONFIGSERVICE = true;
            //注入线程同步服务
            hostBuilder.ConfigureServices((a, b) => collection(a, b));
            return hostBuilder;
        }
        /// <summary>
        /// 注册oxygen配置节
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureOxygen(this IServiceCollection services, IConfiguration configuration)
        {
            //注入默认配置节
            new OxygenSetting(configuration);
            if (CONFIGSERVICE)
            {
                //注入Host启动类
                services.AddHostedService<OxygenHostService>();
            }
            else
            {
                //注入Client启动类
                services.AddHostedService<OxygenClientService>();
            }
            return services;
        }
        //注入方法前、方法后、异常的处理程序
        public static IServiceCollection RegisterPipelineHandler(this IServiceCollection services, Func<object, Task> beforeFunc = null, Func<object, Task> afterFunc = null, Func<Exception, Task<object>> exceptionFunc = null)
        {
            LocalMethodAopProvider.RegisterPipelineHandler(beforeFunc, afterFunc, exceptionFunc);
            return services;
        }
        /// <summary>
        /// 注册oxygen mesh追踪头管道
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>

        public static IApplicationBuilder UseOxygenTrace(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestMiddleware>();
            return builder;
        }
    }
}
