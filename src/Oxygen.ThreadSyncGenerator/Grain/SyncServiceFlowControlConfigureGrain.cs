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
        static HashSet<IFlowControlConfigureObserver> observers = new HashSet<IFlowControlConfigureObserver>();
        static List<IFlowControlConfigureObserver> failobservers = new List<IFlowControlConfigureObserver>();
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

        public async Task<bool> RegisterObserver(IFlowControlConfigureObserver observer)
        {
            observers.Add(observer);
            return await Task.FromResult(true);
        }
    }
}
