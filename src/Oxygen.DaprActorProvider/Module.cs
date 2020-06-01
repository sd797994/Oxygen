using Autofac;
using Oxygen.DaprActorProvider;

namespace Oxygen.DaprActorProvider
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutoSaveModelManager>().As<IAutoSaveModelManager>().SingleInstance();
            //注册actor远程代理和自动保存
            ActorServiceBuilder.RegisterActorAutoModelSave(builder);
        }
    }
}
