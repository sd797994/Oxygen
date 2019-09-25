using Microsoft.Extensions.Hosting;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using Oxygen.CommonTool;
using System;
using Oxygen.ThreadSyncGenerator.Extensions;
using Oxygen.ThreadSyncGenerator.Grains;

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
        public static IHostBuilder UseOrleansSiloService(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseOrleans((context, siloBuilder) =>
            {
                siloBuilder
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = CLUSTERID;
                        options.ServiceId = SERVICEID;
                    })
                   .AddRedisGrainStorage(STORAGENAME)
                   .UseConsulClustering(option => { option.Address = new Uri(OxygenSetting.ConsulAddress); })
                   .ConfigureEndpoints(siloPort: GlobalCommon.GetFreePort(), gatewayPort: GlobalCommon.GetFreePort())
                   .ConfigureApplicationParts(parts => parts.AddApplicationPart(new AssemblyPart(typeof(RedisStorageGrain).Assembly)));
            });
            return hostBuilder;
        }
    }
}
