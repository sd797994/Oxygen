using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using Oxygen.IProxyClientBuilder;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oxygen.ServerProxyFactory
{
    public class RemoteProxyDecorator<T> : DispatchProxy
    {
        public T Create()
        {
            object proxy = Create<T, RemoteProxyDecorator<T>>();
            var type = typeof(T);
            var serviceName = (string)typeof(RemoteServiceAttribute).GetProperty("ServerName")
                       ?.GetValue(type.GetCustomAttribute(typeof(RemoteServiceAttribute)));
            foreach (var method in type.GetMethods())
            {
                var key = method.Name + string.Join("", method.GetParameters().Select(x => x.Name));
                var tmpmod = new MethodDelegateInfo()
                {
                    ServiceName = serviceName,
                    PathName = $"{type.Name}/{method.Name}",
                    MethodInfo = typeof(IRemoteProxyGenerator).GetMethod("SendAsync").MakeGenericMethod(method.GetParameters()[0].ParameterType, method.ReturnParameter.ParameterType.GenericTypeArguments[0])
                };
                ProxyClientBuilder.Remotemethods.Add(method.Name + string.Join("", method.GetParameters().Select(x => x.Name)), tmpmod);
            }
            return (T)proxy;
        }

        IRemoteMethodDelegate GetProxy(string key, out string ServiceName, out string PathName)
        {
            var remotemethod = ProxyClientBuilder.Remotemethods.First(x => x.Key.Equals(key)).Value;
            if (remotemethod.MethodDelegate == null)
                remotemethod.MethodDelegate = (IRemoteMethodDelegate)Activator.CreateInstance(typeof(RemoteMethodDelegate<,>).MakeGenericType(remotemethod.MethodInfo.GetParameters()[0].ParameterType, remotemethod.MethodInfo.ReturnType), remotemethod.MethodInfo, ProxyClientBuilder.ProxyGenerator.Value);
            ServiceName = remotemethod.ServiceName;
            PathName = remotemethod.PathName;
            return remotemethod.MethodDelegate;
        }
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var key = targetMethod.Name + string.Join("", targetMethod.GetParameters().Select(y => y.Name));
            return GetProxy(key, out string ServiceName, out string PathName).Excute(args[0], ServiceName, PathName);
        }
    }
}
