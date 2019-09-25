using Orleans;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator.Grains
{
    /// <summary>
    /// redis存储
    /// </summary>
    public interface IRedisStorageGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 读
        /// </summary>
        /// <param name="stateStore"></param>
        /// <param name="grainStoreKey"></param>
        /// <returns></returns>
        Task<IGrainState> ReadStateAsync(string stateStore, string grainStoreKey);
        /// <summary>
        /// 写
        /// </summary>
        /// <param name="grainType"></param>
        /// <param name="grainId"></param>
        /// <param name="grainState"></param>
        /// <returns></returns>
        Task<string> WriteStateAsync(string grainType, string grainId, IGrainState grainState);
        /// <summary>
        /// 删
        /// </summary>
        /// <param name="stateStore"></param>
        /// <param name="grainStoreKey"></param>
        /// <param name="eTag"></param>
        /// <returns></returns>
        Task DeleteStateAsync(string stateStore, string grainStoreKey, string eTag);
    }
}
