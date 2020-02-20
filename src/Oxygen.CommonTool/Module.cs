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
            //注入scope作用域的CustomerInfo用于传递客户端请求信息
            builder.RegisterType<CustomerInfo>().As<CustomerInfo>().InstancePerLifetimeScope();
        }
    }
}
