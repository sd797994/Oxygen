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
                Configuration = config.Build();
            })
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                //依赖注入oxygen
                builder.RegisterOxygen();
                //依赖注入本地业务
                builder.RegisterType<UserLoginUseCase>().As<IUserLoginUseCase>().InstancePerLifetimeScope();
            })
            .ConfigureServices(services =>
            {
                //添加oxygen管道
                services.AddOxygenServer(Configuration);
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                });
                services.AddAutofac();
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .UseConsoleLifetime();
    }
}
