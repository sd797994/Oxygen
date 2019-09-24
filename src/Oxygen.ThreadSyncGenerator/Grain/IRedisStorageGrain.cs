using Orleans;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator.Grains
{
    public interface IRedisStorageGrain : IGrainWithStringKey
    {
        Task<IGrainState> ReadStateAsync(string stateStore, string grainStoreKey);
        Task<string> WriteStateAsync(string grainType, string grainId, IGrainState grainState);
        Task DeleteStateAsync(string stateStore, string grainStoreKey, string eTag);
    }
}
