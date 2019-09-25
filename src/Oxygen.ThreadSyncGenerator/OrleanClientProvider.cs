using Orleans;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using Oxygen.CommonTool;
using Oxygen.ServerFlowControl.Configure;
using Oxygen.ThreadSyncGenerator.Grains;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator
{
    /// <summary>
    /// orlean客户端提供者
    /// </summary>
    public class OrleanClientProvider
    {
        static readonly string CLUSTERNAME = "ORLEANCLUSTERCLIENT";
        static ConcurrentDictionary<string, IClusterClient> clusterClients = new ConcurrentDictionary<string, IClusterClient>();
        public static async Task<IClusterClient> GetClient(bool reGetClient = false)
        {
            if (reGetClient)
            {
                clusterClients.TryRemove(CLUSTERNAME, out _);
                var client = new ClientBuilder()
                                .Configure<ClusterOptions>(options =>
                                {
                                    options.ClusterId = OrleanHostBuilder.CLUSTERID;
                                    options.ServiceId = OrleanHostBuilder.SERVICEID;
                                })
                                .UseConsulClustering(option => { option.Address = new Uri(OxygenSetting.ConsulAddress); })
                                .ConfigureApplicationParts(parts => parts.AddApplicationPart(new AssemblyPart(typeof(RedisStorageGrain).Assembly)))
                                .Build();
                await client.Connect();
                clusterClients.TryAdd(CLUSTERNAME, client);
                return client;
            }
            else
            {

                if (clusterClients.TryGetValue(CLUSTERNAME, out IClusterClient value))
                {
                    return value;
                }
                else
                {
                    var client = new ClientBuilder()
                                    .Configure<ClusterOptions>(options =>
                                    {
                                        options.ClusterId = OrleanHostBuilder.CLUSTERID;
                                        options.ServiceId = OrleanHostBuilder.SERVICEID;
                                    })
                                    .UseConsulClustering(option => { option.Address = new Uri(OxygenSetting.ConsulAddress); })
                                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(new AssemblyPart(typeof(RedisStorageGrain).Assembly)))
                                    .Build();
                    await client.Connect();
                    clusterClients.TryAdd(CLUSTERNAME, client);
                    return client;
                }
            }
        }
    }
}
