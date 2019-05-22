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
            //注入MediatR
            service.AddMediatR();
            //注入默认启动类
            service.AddHostedService<OxygenClientService>();
            return service;
        }
    }
}
