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
