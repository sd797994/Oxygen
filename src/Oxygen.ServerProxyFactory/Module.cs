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
            //注入远程代理服务
            ProxyClientBuilder.CreateRemoteProxyClientInstance(builder);
        }
    }
}
