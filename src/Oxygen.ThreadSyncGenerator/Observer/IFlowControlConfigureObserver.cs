using Orleans;
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.ThreadSyncGenerator
{
    public interface IFlowControlConfigureObserver : IGrainObserver
    {
        void UpdateFlowControlConfigure(ServiceConfigureInfo configure);
    }
}
