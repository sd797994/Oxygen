using Autofac;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oxygen.CommonTool;

namespace Oxygen
{
    /// <summary>
    /// 配置中心
    /// </summary>
    public static class RpcConfigurationModule
    {
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
            return builder;
        }
        /// <summary>
        /// 注册成为Oxygen服务节点
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddOxygenServer(this IServiceCollection service, IConfiguration configuration)
        {
            //注入默认配置节
            new OxygenSetting(configuration);
            //注入MediatR
            service.AddMediatR();
            //注入Host启动类
            service.AddHostedService<OxygenHostService>();
            return service;
        }

        /// <summary>
        /// 注册成为Oxygen客户端节点
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddOxygenClient(this IServiceCollection service, IConfiguration configuration)
        {
            //注入默认配置节
            new OxygenSetting(configuration);
            //注入Client启动类
            service.AddHostedService<OxygenClientService>();
            return service;
        }
    }
}
