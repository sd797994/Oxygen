using Oxygen.IProxyClientBuilder;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 代理委托
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class RemoteMethodDelegate<Tin, Tout> : IRemoteMethodDelegate where Tout : Task
    {
        private Func<Tin, string, string, Tout> proxyfunc;
        public RemoteMethodDelegate(MethodInfo method, object instence)
        {
            proxyfunc = (Func<Tin, string, string, Tout>)method.CreateDelegate(typeof(Func<Tin, string, string, Tout>), instence);
        }
        public object Excute(object val, string serviceName, string pathName)
        {
            return proxyfunc((Tin)val, serviceName, pathName);
        }
    }
}
