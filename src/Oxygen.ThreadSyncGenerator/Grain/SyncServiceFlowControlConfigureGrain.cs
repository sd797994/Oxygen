using Orleans;
using Oxygen.IServerFlowControl.Configure;
using Oxygen.ThreadSyncGenerator.Grains;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator
{
    /// <summary>
    /// 同步配置节grain
    /// </summary>
    public class SyncServiceFlowControlConfigureGrain : Grain, ISyncServiceFlowControlConfigureGrain
    {
        static ServiceConfigureInfo configure = null;
        static HashSet<IFlowControlConfigureGrainObserver> observers = new HashSet<IFlowControlConfigureGrainObserver>();
        static List<IFlowControlConfigureGrainObserver> failobservers = new List<IFlowControlConfigureGrainObserver>();
        public async Task<ServiceConfigureInfo> GetConfigure()
        {
            return await Task.FromResult(configure);
        }
        public async Task SetConfigure(ServiceConfigureInfo newConfigure)
        {
            configure = newConfigure;
            foreach(var observer in observers)
            {
                try
                {
                    observer.UpdateFlowControlConfigure(newConfigure);
                }
                catch (Exception)
                {
                    failobservers.Add(observer);
                }
            }
            foreach (var observer in failobservers)
            {
                observers.Remove(observer);
            }
            await Task.CompletedTask;
        }

        public async Task RegisterObserver(IFlowControlConfigureGrainObserver observer)
        {
            observers.Add(observer);
            await Task.CompletedTask;
        }
    }
}
