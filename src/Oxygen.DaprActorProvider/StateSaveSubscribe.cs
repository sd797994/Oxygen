using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    /// <summary>
    /// 状态保存订阅器
    /// </summary>
    public class StateSaveSubscribe : INotificationHandler<ActorModel>
    {
        public async Task Handle(ActorModel request, CancellationToken cancellationToken)
        {
            //await StateManager.TryGetStateAsync<ActorModel>(request);
        }
    }
}
