using Autofac;
using Oxygen.IRpcProviderService;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Oxygen.KestrelRpcProviderService
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
