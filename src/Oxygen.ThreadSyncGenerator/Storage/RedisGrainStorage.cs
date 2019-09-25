using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Oxygen.ThreadSyncGenerator.Grains;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator.Storage
{
    /// <summary>
    /// orlaen扩展redisstorage
    /// </summary>
    public class RedisGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private Lazy<ConcurrentDictionary<string, IRedisStorageGrain>> storageGrains
            = new Lazy<ConcurrentDictionary<string, IRedisStorageGrain>>(() => new ConcurrentDictionary<string, IRedisStorageGrain>());
        private IGrainFactory grainFactory;
        public string name { get; private set; }
        public RedisGrainStorage(
            ILogger<RedisGrainStorage> logger,
            IProviderRuntime providerRuntime,
            IOptions<ClusterOptions> clusterOptions,
            ITypeResolver typeResolver,
            IGrainFactory grainFactory,
            string name)
        {
            this.name = name;
            this.grainFactory = grainFactory;
        }
        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            IList<Tuple<string, string>> keys = MakeKeys(grainType, grainReference).ToList();
            string id = MakeStoreKey(keys);
            IRedisStorageGrain storageGrain = GetStorageGrain(id);
            var state = await storageGrain.ReadStateAsync("redis", id);
            if (state != null)
            {
                grainState.ETag = state.ETag;
                grainState.State = state.State;
            }
        }
        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            IList<Tuple<string, string>> keys = MakeKeys(grainType, grainReference).ToList();
            string key = MakeStoreKey(keys);
            IRedisStorageGrain storageGrain = GetStorageGrain(key);
            try
            {
                grainState.ETag = await storageGrain.WriteStateAsync("redis", key, grainState);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            IList<Tuple<string, string>> keys = MakeKeys(grainType, grainReference).ToList();
            string key = MakeStoreKey(keys);
            IRedisStorageGrain storageGrain = GetStorageGrain(key);
            try
            {
                await storageGrain.DeleteStateAsync("redis", key, grainState.ETag);
                grainState.ETag = null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region lifecycle


        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<RedisGrainStorage>(this.name), ServiceLifecycleStage.ApplicationServices, Init, Close);
        }
        private async Task Init(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }


        /// <summary>
        /// Close this provider
        /// </summary>
        private Task Close(CancellationToken token)
        {
            return Task.CompletedTask;
        }
        #endregion
        #region private func
        private static IEnumerable<Tuple<string, string>> MakeKeys(string grainType, GrainReference grain)
        {
            return new[]
            {
                Tuple.Create("GrainType", grainType),
                Tuple.Create("GrainId", grain.ToKeyString())
            };
        }

        private IRedisStorageGrain GetStorageGrain(string id)
        {
            if (!storageGrains.Value.TryGetValue(id, out IRedisStorageGrain storageGrain))
            {
                storageGrain = this.grainFactory.GetGrain<IRedisStorageGrain>(id, null);
                storageGrains.Value.TryAdd(id, storageGrain);
            }
            return storageGrain;
        }
        internal static string MakeStoreKey(IEnumerable<Tuple<string, string>> keys)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var keyPair in keys)
            {
                if (first)
                    first = false;
                else
                    sb.Append("+");

                sb.Append(keyPair.Item1 + "=" + keyPair.Item2);
            }
            return sb.ToString();
        }
        #endregion
    }
}
