using Application.Interface.UseCase.Dto;
using Oxygen.CsharpClientAgent;
using System.Threading.Tasks;

namespace Application.Interface
{
    [RemoteService("ServerSample")]
    public interface IUserLoginUseCase
    {
        Task<ApplicationBaseResult> Login(LoginInput input);
        Task<ApplicationBaseResult> Register(RegisterInput input);
    }
}
