using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Autofac;
using Oxygen.IRpcProviderService;

namespace Oxygen.DotNettyRpcProviderService
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RpcServerProvider>().As<IRpcServerProvider>().SingleInstance();
            builder.RegisterType<RpcClientProvider>().As<IRpcClientProvider>().SingleInstance();
        }
    }
}
