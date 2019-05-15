using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.Common.Logger
{
    public interface IOxygenLogger
    {
        void LogError(string message);
        void LogInfo(string message);
    }
}
