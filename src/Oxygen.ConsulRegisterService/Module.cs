using Autofac;
using Oxygen.IMicroRegisterService;

namespace Oxygen.ConsulRegisterService
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsulCenterService>().As<IRegisterCenterService>().SingleInstance();
        }
    }
}
