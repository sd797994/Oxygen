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
        public static IHostBuilder UseOrleansSiloService(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseOrleans((context, siloBuilder) =>
            {
                SiloIP = GlobalCommon.GetMachineIp().ToString();
                SiloPort = GlobalCommon.GetFreePort();
                GatewayPort = GlobalCommon.GetFreePort(SiloPort);
                siloBuilder
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = CLUSTERID;
                        options.ServiceId = SERVICEID;
                    })
                   .AddRedisGrainStorage(STORAGENAME)
                   .UseConsulClustering(option => { option.Address = new Uri(OxygenSetting.ConsulAddress); })
                   .ConfigureEndpoints(hostname: SiloIP, siloPort: SiloPort, gatewayPort: GatewayPort)
                   .ConfigureApplicationParts(parts => parts.AddApplicationPart(new AssemblyPart(typeof(RedisStorageGrain).Assembly)));
            });
            return hostBuilder;
        }

        public static async Task ClearConsulKV(Func<string, string, Task> func)
        {
            await func(CLUSTERID, $"{SiloIP}:{SiloPort}");
        }
    }
}