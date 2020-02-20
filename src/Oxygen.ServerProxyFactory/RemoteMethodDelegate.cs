using Oxygen.CommonTool;
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
        private Func<Tin, Dictionary<string, string>, string, string, Tout> proxyfunc;
        public RemoteMethodDelegate(MethodInfo method, object instence)
        {
            proxyfunc = (Func<Tin, Dictionary<string, string>, string, string, Tout>)method.CreateDelegate(typeof(Func<Tin, Dictionary<string, string>, string, string, Tout>), instence);
        }
        public object Excute(object val, string serviceName, string pathName)
        {
            CustomerInfo customer = OxygenIocContainer.Resolve<CustomerInfo>();
            return proxyfunc((Tin)val, customer.TraceHeaders, serviceName, pathName);
        }
    }
}
