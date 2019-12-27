using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.IServerProxyFactory
{
    public interface IRemoteMethodDelegate
    {
        object Excute(object val, string serviceName, string pathName);
    }
}
