using Application.Interface;
using Autofac;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using Oxygen.DaprActorProvider;
using System;
using System.Threading.Tasks;

namespace Application.Service
{
    public abstract class ActorBase<T> : OxygenActor<T>
    {
        public ActorBase(ActorService service, ActorId id, ILifetimeScope container) : base(service, id, container) { }

        public async Task<ApplicationBaseResult> DoAsync(bool saveChanges, Func<ApplicationBaseResult, Task> runMethod)
        {
            var result = new ApplicationBaseResult();
            try
            {
                await runMethod(result);
                result.Code = 0;
                if (saveChanges)
                    await base.SaveAll(true);
            }
            catch (Exception e)
            {
                result.Message = "出错了,请稍后再试";
            }
            return result;
        }
    }
}
