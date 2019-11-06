using Autofac;
using System.Linq;

namespace Oxygen.ProxyClientBuilder
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
