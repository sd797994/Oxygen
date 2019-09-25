using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.ThreadSyncGenerator.Storage
{
    /// <summary>
    /// redisstorage工厂
    /// </summary>
    public static class RedisGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<RedisGrainStorageOptions>>();
            var clusterOptions = services.GetProviderClusterOptions(name);
            return ActivatorUtilities.CreateInstance<RedisGrainStorage>(services, Options.Create(optionsMonitor.Get(name)), name, clusterOptions);
        }
    }
}
