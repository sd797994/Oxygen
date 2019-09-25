using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Oxygen.ThreadSyncGenerator
{
    public class FlowControlConfigureGrainObserver: IFlowControlConfigureGrainObserver
    {
        public void UpdateFlowControlConfigure(ServiceConfigureInfo configure)
        {
            OrleanClientProvider.GetConfigureCache().Set(configure.FlowControlCfgKey, configure);
        }
    }
}
