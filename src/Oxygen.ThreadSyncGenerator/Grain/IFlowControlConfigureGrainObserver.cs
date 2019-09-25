using Orleans;
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.ThreadSyncGenerator
{
    public interface IFlowControlConfigureGrainObserver : IGrainObserver
    {
        void UpdateFlowControlConfigure(ServiceConfigureInfo configure);
    }
}
