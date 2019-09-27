using Orleans;
using Oxygen.IServerFlowControl.Configure;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator.Grains
{
    /// <summary>
    /// 同步配置节grain
    /// </summary>
    public interface ISyncServiceFlowControlConfigureGrain : IGrainWithStringKey
    {
        Task<ServiceConfigureInfo> GetConfigure();
        Task SetConfigure(ServiceConfigureInfo newConfigure);
        Task RegisterObserver(IFlowControlConfigureObserver observer);
    }
}
