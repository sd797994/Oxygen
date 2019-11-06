using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.IProxyClientBuilder
{
    public interface IRemoteMethodDelegate
    {
        object Excute(object val, string serviceName, string pathName);
    }
}
