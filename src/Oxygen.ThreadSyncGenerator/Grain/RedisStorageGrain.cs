using Orleans;
using Oxygen.ICache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator.Grains
{
    /// <summary>
    /// redis存储
    /// </summary>
    public class RedisStorageGrain : Grain, IRedisStorageGrain
    {
        private IDictionary<string, GrainStateStore> grainStore;
        public readonly ICacheService _cacheService;
        public RedisStorageGrain(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }
        public override Task OnActivateAsync()
        {
            grainStore = new Dictionary<string, GrainStateStore>();
            base.DelayDeactivation(TimeSpan.FromDays(10 * 365));
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            grainStore = null;
            return Task.CompletedTask;
        }

        public Task<IGrainState> ReadStateAsync(string stateStore, string grainStoreKey)
        {
            GrainStateStore storage = GetStoreForGrain(stateStore);
            var grainState = storage.GetGrainState(grainStoreKey, _cacheService);
            return Task.FromResult(grainState);
        }

        public Task<string> WriteStateAsync(string stateStore, string grainStoreKey, IGrainState grainState)
        {
            GrainStateStore storage = GetStoreForGrain(stateStore);
            storage.UpdateGrainState(grainStoreKey, grainState, _cacheService);
            return Task.FromResult(grainState.ETag);
        }

        public Task DeleteStateAsync(string grainType, string grainId, string etag)
        {
            GrainStateStore storage = GetStoreForGrain(grainType);
            storage.DeleteGrainState(grainId, etag, _cacheService);
            return Task.CompletedTask;
        }

        private GrainStateStore GetStoreForGrain(string grainType)
        {
            GrainStateStore storage;
            if (!grainStore.TryGetValue(grainType, out storage))
            {
                storage = new GrainStateStore();
                grainStore.Add(grainType, storage);
            }

            return storage;
        }

        private class GrainStateStore
        {
            public IGrainState GetGrainState(string grainId, ICacheService cacheService)
            {
                IGrainState entry = cacheService.GetCache<IGrainState>(grainId);
                return ReferenceEquals(entry, Deleted) ? null : entry;
            }

            public void UpdateGrainState(string grainId, IGrainState grainState, ICacheService cacheService)
            {
                IGrainState entry = cacheService.GetCache<IGrainState>(grainId);
                string currentETag = null;
                if (entry != null)
                {
                    currentETag = entry.ETag;
                }

                ValidateEtag(currentETag, grainState.ETag, grainId, "Update");
                grainState.ETag = NewEtag();
                cacheService.SetCache(grainId, grainState);
            }

            public void DeleteGrainState(string grainId, string receivedEtag, ICacheService cacheService)
            {
                IGrainState entry = cacheService.GetCache<IGrainState>(grainId);
                string currentETag = null;
                if (entry != null)
                {
                    currentETag = entry.ETag;
                }

                ValidateEtag(currentETag, receivedEtag, grainId, "Delete");
                cacheService.SetCache(grainId, Deleted);
            }

            private static string NewEtag()
            {
                return Guid.NewGuid().ToString("N");
            }

            private void ValidateEtag(string currentETag, string receivedEtag, string grainStoreKey, string operation)
            {
                if (currentETag == null)
                    return;
                if (string.IsNullOrEmpty(currentETag) && receivedEtag == null)
                    return;
                if (receivedEtag == currentETag || receivedEtag == "*")
                    return;
                throw new Exception($"Error For ValidateEtag,currentETag:{currentETag},receivedEtag:{receivedEtag}");
            }

            /// <summary>
            /// Marker to record deleted state so we can detect the difference between deleted state and state that never existed.
            /// </summary>
            private class DeletedState : IGrainState
            {
                public DeletedState()
                {
                    ETag = string.Empty;
                }
                public object State { get; set; }
                public Type Type => typeof(object);
                public string ETag { get; set; }
            }
            private static readonly IGrainState Deleted = new DeletedState();
        }
    }
}
