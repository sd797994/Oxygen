using Application.Interface;
using Application.Service;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oxygen;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Server.Sample
{
    class Program
    {
        private static IConfiguration Configuration { get; set; }
        static async Task Main(string[] args)
        {
            await CreateDefaultHost(args).Build().RunAsync();
        }
        static IHostBuilder CreateDefaultHost(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                //获取oxygen配置节
                config.AddJsonFile("oxygen.json");
                Configuration = config.Build();
            })
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                //注入oxygen依赖
                builder.RegisterOxygen();
                //注入本地业务依赖 
                builder.RegisterType<UserLoginUseCase>().As<IUserLoginUseCase>().InstancePerLifetimeScope();

            })
            //注册成为oxygen服务节点
            .UseOxygenService((context, services) =>
            {
                //注册oxygen配置
                services.ConfigureOxygen(Configuration);
                services.RegisterPipelineHandler(async (obj) =>
                {
                    Console.WriteLine($"这里是方法前拦截器，拦截到参数：{JsonSerializer.Serialize(obj)}");
                    await Task.CompletedTask;
                }, async (result) =>
                {
                    Console.WriteLine($"这里是方法后拦截器，拦截到方法结果：{JsonSerializer.Serialize(result)}");
                    await Task.CompletedTask;
                }, async (exp) =>
                {
                    Console.WriteLine($"这里是方法异常拦截器，拦截到异常：{exp.Message}");
                    return await Task.FromResult(new ApplicationBaseResult() { Message = exp.Message });
                });
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                });
                services.AddAutofac();
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
