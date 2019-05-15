using System;
using System.Collections.Generic;
using System.Text;
using Autofac;

namespace Oxygen.ServerProxyFactory
{

    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
