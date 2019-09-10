using Application.Interface.UseCase.Dto;
using Oxygen.CsharpClientAgent;
using System.Threading.Tasks;

namespace Application.Interface
{
    [RemoteService("ServerSample")]
    public interface IUserLoginUseCase
    {
        [FlowControl("Login")]
        Task<ApplicationBaseResult> Login(LoginInput input);
        [FlowControl("Register")]
        Task<ApplicationBaseResult> Register(RegisterInput input);
    }
}
