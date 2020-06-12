using Application.Interface;
using Application.Service;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oxygen;
using System.IO;
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
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                });
                services.AddAutofac();
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
