using Dapr.Actors.Runtime;
using MediatR;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider.StateManage
{
    /// <summary>
    /// 状态保存订阅器
    /// </summary>
    public class ActorStateSubscriber : INotificationHandler<ActorStateMessage>
    {
        public static Lazy<PropertyInfo> StateManagerProperty = new Lazy<PropertyInfo>(() => typeof(Actor).GetProperty("StateManager", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        public static Lazy<PropertyInfo> ActorInstanceProperty = new Lazy<PropertyInfo>(() => typeof(OxygenActorBase).GetProperty("baseinstance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        public static Lazy<MethodInfo> InstanceSaveMethod = new Lazy<MethodInfo>(() => typeof(OxygenActorBase).GetMethod("SaveInstance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Func<Task> localfunc;
        public async Task Handle(ActorStateMessage request, CancellationToken cancellationToken)
        {
            var actor = request.Actor;
            var stateManager = StateManagerProperty.Value.GetValue(actor);
            var instance = ActorInstanceProperty.Value.GetValue(actor);
            await ((IActorStateManager)stateManager).AddOrUpdateStateAsync(actor.Id.GetId(), instance, (x, y) => y);
            if (request.AutoSave)
            {
                if (localfunc == null)
                    localfunc = (Func<Task>)InstanceSaveMethod.Value.CreateDelegate(typeof(Func<Task>), actor);
                await localfunc();
            }
        }
    }
}
