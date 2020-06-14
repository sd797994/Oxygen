using Autofac;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using MediatR;
using Oxygen.DaprActorProvider.StateManage;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    public abstract class OxygenActorBase : Actor
    {
        protected object baseinstance { get; set; }
        public bool AutoSave { get; set; }
        public OxygenActorBase(ActorService actorService, ActorId actorId)
              : base(actorService, actorId)
        {

        }
        protected abstract Task SaveInstance();
    }
    public abstract class OxygenActor<T> : OxygenActorBase
    {
        private ActorId actorId;
        private readonly ILifetimeScope container;
        public OxygenActor(ActorService actorService, ActorId actorId, ILifetimeScope container)
              : base(actorService, actorId)
        {
            this.actorId = actorId;
            this.container = container;
        }
        private T _instance;
        public T instance { get { return _instance; } protected set { _instance = value; baseinstance = value; } }
        /// <summary>
        /// actor被创建，需要从持久化设备恢复之前的状态
        /// </summary>
        /// <returns></returns>
        protected override async Task OnActivateAsync()
        {
            var result = await StateManager.TryGetStateAsync<T>(actorId.GetId());
            if (result.HasValue)
            {
                instance = result.Value;
            }
            await base.OnActivateAsync();
        }
        /// <summary>
        /// actor被显式的释放时应该持久化状态
        /// </summary>
        /// <returns></returns>
        protected override async Task OnDeactivateAsync()
        {
            if (instance != null)
            {
                await StateManager.TryAddStateAsync(actorId.GetId(), instance);
            }
            await base.OnDeactivateAsync();
        }
        /// <summary>
        /// 官方AOP实现，用于异步发布消息到订阅器进行持久化
        /// </summary>
        /// <param name="actorMethodContext"></param>
        /// <returns></returns>
        protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            if (AutoSave)
                return Task.Run(async () => await container.Resolve<IMediator>().Publish(new ActorStateMessage(this)));
            return Task.CompletedTask;
        }
    }
}
