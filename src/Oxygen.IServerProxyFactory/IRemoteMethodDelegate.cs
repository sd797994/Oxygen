using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.IServerProxyFactory
{
    public interface IRemoteMethodDelegate
    {
        object Excute(object val, Dictionary<string, string> traceHeaders, string serviceName, string pathName);
    }
}
