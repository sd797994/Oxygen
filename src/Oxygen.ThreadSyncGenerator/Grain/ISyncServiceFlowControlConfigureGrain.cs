using Orleans;
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ThreadSyncGenerator.Grains
{
    public interface ISyncServiceFlowControlConfigureGrain : IGrainWithStringKey
    {
        Task<TokenBucketInfo> GetBucket();
        Task SetBucket(TokenBucketInfo newBucket);
        Task<ServiceConfigureInfo> GetConfigure();
        Task SetConfigure(ServiceConfigureInfo newConfigure);
    }
}
