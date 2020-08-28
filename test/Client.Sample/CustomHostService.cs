﻿using Application.Interface;
using Application.Interface.Interfaces;
using Application.Interface.UseCase.Dto;
using Microsoft.Extensions.Hosting;
using Oxygen.CommonTool;
using Oxygen.DaprActorProvider;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Sample
{
    public class CustomHostService : IHostedService
    {
        private readonly IServerProxyFactory _proxyFactory;
        public CustomHostService(IServerProxyFactory proxyFactory)
        {
            _proxyFactory = proxyFactory;
        }
        static int succ = 0;
        static int fail = 0;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //测试调用
            Console.WriteLine("按任意键开始测试....");
            Console.ReadLine();
            EventWaitHandle _event = new AutoResetEvent(false);
            var callCount = 1;
            while (true)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                succ = 0;
                fail = 0;
                await fortest(1, callCount, async i =>
                {
                    var userserver = _proxyFactory.CreateProxy<IUserLoginUseCase>();
                    var result1 = await userserver.Register(new RegisterInput() { UserName = "admin" });
                    //var remoteProxy = _proxyFactory.CreateProxy("/api/serversample/UserLoginUseCase/register");
                    //var result1 = await remoteProxy.SendAsync(new RegisterInput() { UserName = "admin" });
                    if (result1 == null)
                    {
                        Interlocked.Increment(ref fail);
                    }
                    else
                    {
                        Interlocked.Increment(ref succ);
                    }
                    if (fail + succ == callCount)
                    {
                        _event.Set();
                        sw.Stop();
                    }
                });
                _event.WaitOne();
                Console.WriteLine($"RPC调用{callCount}次,成功{succ}次，失败{fail}次，累计耗时{sw.ElapsedMilliseconds}ms");
                Console.WriteLine("按任意键继续按q退出....");
                var returncode = Console.ReadLine();
                if (returncode == "q")
                {
                    break;
                }
                else if (int.TryParse(returncode, out int newcount))
                {
                    callCount = newcount;
                }
                else
                {
                    callCount = 1;
                }
            }
        }
        async Task fortest(int type, int callCount, Func<int, Task> action)
        {
            if (type == 0)
            {
                for (var i = 0; i < callCount; i++)
                {
                    await action.Invoke(i);
                }
            }
            else if (type == 1)
            {
                Parallel.For(0, callCount, async i =>
                  {
                      await action.Invoke(i);
                  });
            }
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
    public class BaseDto
    {
        public Guid ID { get; set; }
    }
    public class AAA : BaseDto
    {

    }
}
