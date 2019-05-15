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
