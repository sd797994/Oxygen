using Autofac;
using Orleans;
using Orleans.Storage;
using System.Linq;
using System.Reflection;

namespace Oxygen.ServerFlowControl.Configure
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
