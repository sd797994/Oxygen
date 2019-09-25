using Microsoft.Extensions.Caching.Memory;
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
        static Lazy<MemoryCache> memoryCache = new Lazy<MemoryCache>(() => new MemoryCache(new MemoryCacheOptions()));
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

        /// <summary>
        /// 获取grain对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="reGetClient"></param>
        /// <returns></returns>
        public static async Task<ISyncServiceFlowControlConfigureGrain> GetGrain(string key, bool reGetClient = false)
        {
            try
            {
                var client = await GetClient(reGetClient);
                if (client != null)
                {
                    return client.GetGrain<ISyncServiceFlowControlConfigureGrain>(key);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static MemoryCache GetConfigureCache()
        {
            return memoryCache.Value;
        }
    }
}
