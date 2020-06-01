using Application.Interface.UseCase.Dto;
using Dapr.Actors;
using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Interfaces
{

    [ActorService(true)]
    public interface IUserActorService : IActor
    {
        Task<ApplicationBaseResult> Login(LoginInput input);
        Task<ApplicationBaseResult> Register(RegisterInput input);
    }
}
