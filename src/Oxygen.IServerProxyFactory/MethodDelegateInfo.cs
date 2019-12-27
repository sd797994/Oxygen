using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Oxygen.IServerProxyFactory
{
    public class MethodDelegateInfo
    {
        public string ServiceName { get; set; }
        public string PathName { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public IRemoteMethodDelegate MethodDelegate { get; set; }
    }
}
