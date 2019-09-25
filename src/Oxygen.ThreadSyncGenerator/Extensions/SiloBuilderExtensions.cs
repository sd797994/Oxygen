using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Oxygen.ThreadSyncGenerator.Storage;
using System;

namespace Oxygen.ThreadSyncGenerator.Extensions
{
    /// <summary>
    /// orlaen扩展redisstorage
    /// </summary>
    public static class SiloBuilderExtensions
    {
        public static ISiloBuilder AddRedisGrainStorage(this ISiloBuilder builder, string name)
        {
            return builder.ConfigureApplicationParts(delegate (IApplicationPartManager parts)
            {
                parts.AddFrameworkPart(typeof(RedisGrainStorage).Assembly);
            }).ConfigureServices(delegate (IServiceCollection services)
            {
                services.ConfigureNamedOptionForLogging<MemoryGrainStorageOptions>(name);
                if (string.Equals(name, "Default"))
                {
                    ServiceCollectionDescriptorExtensions.TryAddSingleton<IGrainStorage>(services, (Func<IServiceProvider, IGrainStorage>)((IServiceProvider sp) => sp.GetServiceByName<IGrainStorage>("Default")));
                }
                services.AddSingletonNamedService(name, RedisGrainStorageFactory.Create);
            });
        }
    }
}
