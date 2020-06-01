using Castle.DynamicProxy;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.DaprActorProvider
{
    /// <summary>
    /// actor自动保存管理器
    /// </summary>
    public class AutoSaveModelManager : IAutoSaveModelManager, IInterceptor
    {
        private readonly IMediator mediator;
        public AutoSaveModelManager(IMediator mediator)
        {
            this.mediator = mediator;
        }
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            mediator.Publish(new ActorModel());
        }
    }
}
