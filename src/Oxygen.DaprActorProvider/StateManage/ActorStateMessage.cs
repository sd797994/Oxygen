using Dapr.Actors.Runtime;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.DaprActorProvider.StateManage
{
    public class ActorStateMessage : INotification
    {
        public ActorStateMessage(object actor)
        {
            Actor = (Actor)actor;
        }
        public Actor Actor { get; set; }
        public IActorStateManager StateManager { get; set; }
    }
}
