using Autofac;

namespace Oxygen.DaprActorProvider
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //创建actor服务代理
            ActorServiceBuilder.RegisterActorToContainer(builder);
        }
    }
}
