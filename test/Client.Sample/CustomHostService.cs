﻿using Application.Interface;
using Microsoft.Extensions.Hosting;
using Oxygen.IServerProxyFactory;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface.UseCase.Dto;

namespace Client.Sample
{
    public class CustomHostService : IHostedService
    {
        private readonly IServerProxyFactory _proxyFactory;
        public CustomHostService(IServerProxyFactory proxyFactory)
        {
            _proxyFactory = proxyFactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ////测试调用
            //Thread.Sleep(1000);
            //while (true)
            //{
            //    Stopwatch sw = new Stopwatch();
            //    sw.Start();
            //    for (int i = 0; i < 10000; i++)
            //    {
            //var userserver = await _proxyFactory.CreateProxy<IUserLoginUseCase>();
            //var result1 = await userserver.Register(new RegisterInput() { UserName = "admin" });
            //var result2 = await userserver.Login(new LoginInput() { UserName = "admin" });

            //var registerServer = await _proxyFactory.CreateProxy("/api/UserService/UserLoginUseCase/Register");
            //var result1 = await registerServer.SendAsync(new LoginInput() { UserName = "admin" });
            //var loginServer = await _proxyFactory.CreateProxy("/api/UserService/UserLoginUseCase/Login");
            //var result2 = await loginServer.SendAsync(new LoginInput() { UserName = "admin" });
            //    }
            //    sw.Stop();
            //    Console.WriteLine($"一万次调用IUserLoginUseCase，耗时{sw.ElapsedMilliseconds}ms");
            //    Console.WriteLine("按任意键继续按q退出....");
            //    if (Console.ReadLine() == "q")
            //    {
            //        break;
            //    }
            //}
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}