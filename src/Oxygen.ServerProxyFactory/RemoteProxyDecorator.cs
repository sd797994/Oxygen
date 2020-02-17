using Oxygen.CommonTool;
using Oxygen.IServerProxyFactory;
using System;
using System.Linq;
using System.Reflection;

namespace Oxygen.ServerProxyFactory
{
    public class RemoteProxyDecorator<T> : DispatchProxy
    {
        public CustomerInfo customer { get; set; }
        IRemoteMethodDelegate GetProxy(string key, out string ServiceName, out string PathName)
        {
            var remotemethod = ProxyClientBuilder.Remotemethods.First(x => x.Key.Equals(key)).Value;
            if (remotemethod.MethodDelegate == null)
                remotemethod.MethodDelegate = (IRemoteMethodDelegate)Activator.CreateInstance(typeof(RemoteMethodDelegate<,>).MakeGenericType(remotemethod.MethodInfo.GetParameters()[0].ParameterType, remotemethod.MethodInfo.ReturnType), remotemethod.MethodInfo,
                    ProxyClientBuilder.ProxyGenerator.Value
                    );
            ServiceName = remotemethod.ServiceName;
            PathName = remotemethod.PathName;
            return remotemethod.MethodDelegate;
        }
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            return GetProxy($"{targetMethod.DeclaringType.Name}/{targetMethod.Name}", out string ServiceName, out string PathName).Excute(args[0], customer.TraceHeaders, ServiceName, PathName);
        }
    }
}
