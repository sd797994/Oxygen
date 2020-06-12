using Autofac;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using Oxygen.CommonTool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    public abstract class OxygenActorBase : Actor
    {
        protected object baseinstance { get; set; }
        public OxygenActorBase(ActorService actorService, ActorId actorId)
              : base(actorService, actorId)
        {

        }
    }
    public abstract class OxygenActor<T> : OxygenActorBase
    {
        private ActorId actorId;
        public OxygenActor(ActorService actorService, ActorId actorId, ILifetimeScope container)
              : base(actorService, actorId)
        {
            this.actorId = actorId;
        }
        private T _instance;
        public new T instance { get { return _instance; } protected set { _instance = value; baseinstance = value; } }
        protected override async Task OnActivateAsync()
        {
            var result = await StateManager.TryGetStateAsync<T>(actorId.GetId());
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
