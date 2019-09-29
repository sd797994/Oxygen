using Microsoft.Extensions.Hosting;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using Oxygen.CommonTool;
using System;
using Oxygen.ThreadSyncGenerator.Extensions;
using Oxygen.ThreadSyncGenerator.Grains;
using System.Threading.Tasks;

namespace Oxygen.ServerFlowControl.Configure
{
    /// <summary>
    /// orlean服务端builder
    /// </summary>
    public static class OrleanHostBuilder
    {
        public static readonly string CLUSTERID = "OXYGENTHREADSYNCSERVICE";
        public static readonly string SERVICEID = "OXYGENTHREADSYNCCLUSTER";
        static readonly string STORAGENAME = "OXYGENTHREADSYNCREDISSTORAGE";
        public static int SiloPort;
        public static string SiloIP;
        public static int GatewayPort;
        static Lazy<ISiloHost> siloHost = new Lazy<ISiloHost>(() =>
        {
            SiloIP = GlobalCommon.GetMachineIp().ToString();
            SiloPort = GlobalCommon.GetFreePort();
            GatewayPort = GlobalCommon.GetFreePort(SiloPort);
            return new SiloHostBuilder()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = CLUSTERID;
                options.ServiceId = SERVICEID;
            })
            .AddRedisGrainStorage(STORAGENAME)
            .UseConsulClustering(option => { option.Address = new Uri(OxygenSetting.ConsulAddress); })
            .ConfigureEndpoints(hostname: SiloIP, siloPort: SiloPort, gatewayPort: GatewayPort)
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(new AssemblyPart(typeof(RedisStorageGrain).Assembly)))
            .Build();
        });
        public static IHostBuilder UseOrleansSiloService(this IHostBuilder hostBuilder)
        {
            _ = Task.Run(async () => await siloHost.Value.StartAsync());
            return hostBuilder;
        }

        public static async Task CloseOrleansSiloService(Func<string, string, Task> func)
        {
            await siloHost.Value.StopAsync();
            await func(CLUSTERID, $"{SiloIP}:{SiloPort}");
        }
    }
}