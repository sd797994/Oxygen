using Oxygen.CommonTool;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Oxygen.ServerProxyFactory
{
    public class RemoteProxyDecorator<T> : DispatchProxy
    {
        IRemoteMethodDelegate GetProxy(string key, out string ServiceName, out string PathName)
        {
            var remotemethod = RemoteProxyDecoratorBuilder.Remotemethods.First(x => x.Key.Equals(key)).Value;
            if (remotemethod.MethodDelegate == null)
                remotemethod.MethodDelegate = (IRemoteMethodDelegate)Activator.CreateInstance(typeof(RemoteMethodDelegate<,>).MakeGenericType(remotemethod.MethodInfo.GetParameters()[0].ParameterType, remotemethod.MethodInfo.ReturnType), remotemethod.MethodInfo,
                    OxygenIocContainer.Resolve<IRemoteProxyGenerator>()
                    );
            ServiceName = remotemethod.ServiceName;
            PathName = remotemethod.PathName;
            return remotemethod.MethodDelegate;
        }
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            return GetProxy($"/{targetMethod.DeclaringType.Name}/{targetMethod.Name}", out string ServiceName, out string PathName).Excute(args[0], ServiceName, PathName);

        }
    }
}
