using Oxygen.CsharpClientAgent;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace Oxygen.ProxyClientBuilder
{
    public class RemoteClientProxyBase<T> : DynamicObject
    {
        string FlowControlCfgKey { get; set; }
        object ServiceName { get; set; }
        string PathName { get; set; }
        Type InterFaceType { get; set; }
        private readonly IRemoteProxyGenerator _proxyGenerator;
        public RemoteClientProxyBase(IRemoteProxyGenerator proxyGenerator)
        {
            _proxyGenerator = proxyGenerator;
            InterFaceType = typeof(T);
            ServiceName = typeof(RemoteServiceAttribute).GetProperty("ServerName")
                ?.GetValue(InterFaceType.GetCustomAttribute(typeof(RemoteServiceAttribute)));
            PathName = $"{InterFaceType.Name.Substring(1, InterFaceType.Name.Length - 1)}_parameterType.Name";

        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            return true;
            //binder.GetType
            //result = _proxyGenerator.SendAsync<object, object>(args, ServerName, FlowControlCfgKey, PathName)
            //return true;
        }
    }
}
