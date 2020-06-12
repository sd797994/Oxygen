using Autofac;
using Dapr.Actors.Runtime;
using MediatR;
using Oxygen.CommonTool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider.Aspect
{
    public class ActorStateSavePublisher
    {
        public static void Publish(object actor, ILifetimeScope container)
        {
            _ = Task.Run(async () => 
            {
                await container.Resolve<IMediator>().Publish(new ActorStateMessage(actor));
            });
        }
    }
}
