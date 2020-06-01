using Application.Interface;
using Application.Interface.Interfaces;
using Application.Interface.UseCase.Dto;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using Oxygen.DaprActorProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service.Actor
{
    public class UserActorService : ActorBase, IUserActorService
    {
        public static List<string> UserNames = new List<string>();
        public UserActorService(ActorService service, ActorId id) : base(service, id) { }
        public async Task<ApplicationBaseResult> Login(LoginInput input)
        {
            return await DoAsync(async (x) =>
            {
                await Task.Delay(0);
                if (UserNames.Any(y => y.Equals(input.UserName)))
                {
                    x.Code = 0;
                    x.Message = "登录成功";
                }
                else
                {
                    x.Code = 500;
                    x.Message = "登录失败";
                }
            });
        }

        public async Task<ApplicationBaseResult> Register(RegisterInput input)
        {
            return await DoAsync(async (x) =>
            {
                await Task.Delay(0);
                if (string.IsNullOrEmpty(input.UserName))
                {
                    x.Code = -1;
                    x.Message = "请输入用户名";
                }
                if (UserNames.Any(y => y.Equals(input.UserName)))
                {
                    x.Code = -1;
                    x.Message = "该用户注册过了";
                }
                else
                {
                    UserNames.Add(input.UserName);
                    x.Code = 0;
                    x.Message = "注册成功";
                }
            });
        }
    }
}
