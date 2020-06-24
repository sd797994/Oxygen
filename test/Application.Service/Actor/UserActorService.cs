using Application.Interface;
using Application.Interface.Interfaces;
using Application.Interface.UseCase.Dto;
using Autofac;
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
    public class UserActorService : ActorBase<LoginInput>, IUserActorService
    {
        public UserActorService(ActorService service, ActorId id, ILifetimeScope container) : base(service, id, container) {  }
        public async Task<ApplicationBaseResult> Login(LoginInput input)
        {
            return await DoAsync(input.SaveChanges, async (x) =>
            {
                await Task.Delay(0);
                if (instance != null && instance.UserName.Equals(input.UserName))
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
            return await DoAsync(input.SaveChanges, async (x) =>
            {
                await Task.Delay(0);
                if (string.IsNullOrEmpty(input.UserName))
                {
                    x.Code = -1;
                    x.Message = "请输入用户名";
                }
                if (instance != null && instance.UserName.Equals(input.UserName))
                {
                    x.Code = -1;
                    x.Message = "该用户注册过了";
                }
                else
                {
                    instance = new LoginInput() { UserName = input.UserName };
                    x.Code = 0;
                    x.Message = "注册成功";
                }
            });
        }
        /// <summary>
        /// 提供抽象方法SaveInstance的实现用于自动保存时调用仓储进行持久化
        /// </summary>
        /// <returns></returns>
        protected override async Task SaveInstance()
        {
            //在真实环境中调用repo进行数据持久化
            Console.WriteLine("SaveInstance success");
        }
    }
}