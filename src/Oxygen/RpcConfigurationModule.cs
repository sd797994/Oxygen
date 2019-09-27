using Autofac;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Oxygen.CommonTool;
using Oxygen.ServerFlowControl.Configure;
using System;

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
            builder.RegisterModule(new DotNettyRpcProviderService.Module());
            //注入代理构造服务
            builder.RegisterModule(new ProxyClientBuilder.Module());
            //注入序列化服务
            builder.RegisterModule(new MessagePackSerializeService.Module());
            //注入代理工厂
            builder.RegisterModule(new ServerProxyFactory.Module());
            //注入通用服务
            builder.RegisterModule(new CommonTool.Module());
            //注入注册中心服务
            builder.RegisterModule(new ConsulServerRegisterManage.Module());
            //注入缓存服务
            builder.RegisterModule(new RedisCache.Module());
            //注入流控服务
            builder.RegisterModule(new ServerFlowControl.Module());
            //注入流控配置服务
            builder.RegisterModule(new ServerFlowControl.Configure.Module());
            return builder;
        }
        /// <summary>
        /// 注册成为Oxygen服务节点
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IHostBuilder UseOxygenService(this IHostBuilder hostBuilder, Action<IServiceCollection> collection)
        {
            CONFIGSERVICE = true;
            //注入线程同步服务
            hostBuilder.UseOrleansSiloService().ConfigureServices(x => collection(x));
            return hostBuilder;
        }
        /// <summary>
        /// 注册oxygen配置节
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureOxygen(this IServiceCollection service, IConfiguration configuration)
        {
            //注册默认周期管理
            service.AddSingleton<IHostLifetime, ConsoleLifetime>();
            //注入默认配置节
            new OxygenSetting(configuration);
            if (CONFIGSERVICE)
            {
                //注入MediatR
                service.AddMediatR();
                //注入Host启动类
                service.AddHostedService<OxygenHostService>();
            }
            else
            {
                //注入Client启动类
                service.AddHostedService<OxygenClientService>();
            }
            return service;
        }
    }
}
