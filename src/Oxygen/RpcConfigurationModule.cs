using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Configuration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Oxygen.Common;
using Oxygen.Common.Logger;

namespace Oxygen
{
    public static class RpcConfigurationModule
    {
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
            //注入服务发现
            builder.RegisterModule(new ConsulRegisterService.Module());
            //注入通用服务
            builder.RegisterModule(new Common.Module());
            return builder;
        }

        /// <summary>
        /// 注册成为客户端
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddOxygenClient(this IServiceCollection service, IConfiguration configuration)
        {
            //注入全局配置节
            OxygenSetting.Init(configuration);
            return service;
        }
        /// <summary>
        /// 注册成为服务器
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddOxygenServer(this IServiceCollection service, IConfiguration configuration)
        {
            //注入全局配置节
            OxygenSetting.Init(configuration);
            //注入MediatR
            service.AddMediatR();
            //注入默认启动类
            service.AddHostedService<OxygenClientService>();
            return service;
        }
    }
}
