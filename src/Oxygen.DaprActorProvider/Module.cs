using Autofac;
using MediatR;
using Oxygen.DaprActorProvider.Aspect;
using Oxygen.IRpcProviderService;
using System.Collections.Generic;

namespace Oxygen.DaprActorProvider
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //创建actor服务代理
            ActorServiceBuilder.RegisterActorToContainer();
            //注册MediatR用于actor服务自动保存
            ActorServiceBuilder.RegisterProxyMediatR(builder);
        }
    }
}
