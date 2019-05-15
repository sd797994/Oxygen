using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace Oxygen.ProxyClientBuilder
{

    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            var locals = ProxyClientBuilder.CreateLocalProxyClientBuilder();
            var remote = ProxyClientBuilder.CreateRemoteProxyClientBuilder();
            //为本地服务构建代理类
            if (locals != null && locals.Any())
            {
                builder.RegisterTypes(locals.ToArray()).AsImplementedInterfaces().InstancePerLifetimeScope();
            }
            //为远程服务构建代理类
            if (remote != null && remote.Any())
            {
                builder.RegisterTypes(remote.ToArray()).AsImplementedInterfaces().InstancePerLifetimeScope();
            }
            
        }
    }
}
