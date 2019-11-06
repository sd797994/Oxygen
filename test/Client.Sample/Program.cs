using System;
using System.IO;
using System.Threading.Tasks;
using Application.Interface;
using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oxygen;

namespace Client.Sample
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
            })
            .ConfigureServices(services =>
            {
                //注册oxygen配置节
                services.ConfigureOxygen(Configuration);
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                });
                services.AddHostedService<CustomHostService>();
                services.AddHttpClient();
                services.AddAutofac();
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
