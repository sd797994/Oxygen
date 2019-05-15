using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Autofac;

namespace Oxygen.Common
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
