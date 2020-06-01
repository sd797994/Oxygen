using Application.Interface;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using Oxygen.DaprActorProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public abstract class ActorBase : OxygenActor
    {
        public ActorBase(ActorService service, ActorId id) : base(service, id) { }

        public async Task<ApplicationBaseResult> DoAsync(Func<ApplicationBaseResult, Task> runMethod)
        {
            var result = new ApplicationBaseResult();
            try
            {
                await runMethod(result);
                result.Code = 0;
            }
            catch (Exception e)
            {
                result.Message = "出错了,请稍后再试";
            }
            return result;
        }
    }
}
