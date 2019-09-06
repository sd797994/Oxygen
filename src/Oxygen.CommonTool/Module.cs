using Autofac;

namespace Oxygen.CommonTool
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            //注入scope作用域的CustomerIp用于传递客户端IP
            builder.RegisterType<CustomerIp>().As<CustomerIp>().InstancePerLifetimeScope();
        }
    }
}
