using Dapr.Actors;
using Dapr.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    public abstract class OxygenActor : Actor
    {
        private ActorId actorId;
        public OxygenActor(ActorService actorService, ActorId actorId)
              : base(actorService, actorId)
        {
            this.actorId = actorId;
        }
        protected ActorModel instance;
        protected override async Task OnActivateAsync()
        {
            var result = await StateManager.TryGetStateAsync<ActorModel>(actorId.GetId());
            if (result.HasValue)
            {
                instance = result.Value;
            }
            await base.OnActivateAsync();
        }

        protected override async Task OnDeactivateAsync()
        {
            if (instance != null)
            {
                await StateManager.TryAddStateAsync(actorId.GetId(), instance);
            }
            await base.OnDeactivateAsync();
        }
    }
}
