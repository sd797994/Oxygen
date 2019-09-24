using Orleans;
using Oxygen.IServerFlowControl.Configure;
using Oxygen.ThreadSyncGenerator.Grains;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator
{
    public class SyncServiceFlowControlConfigureGrain : Grain, ISyncServiceFlowControlConfigureGrain
    {
        static ServiceConfigureInfo configure = null;
        public async Task<ServiceConfigureInfo> GetConfigure()
        {
            return await Task.FromResult(configure);
        }
        public async Task SetConfigure(ServiceConfigureInfo newConfigure)
        {
            configure = newConfigure;
            await Task.CompletedTask;
        }
    }
}
