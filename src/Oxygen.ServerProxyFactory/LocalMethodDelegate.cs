using Oxygen.IProxyClientBuilder;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IProxyClientBuilder
{
    /// <summary>
    /// 代理委托
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class LocalMethodDelegate<Tin, Tout> : ILocalMethodDelegate
    {
        private Func<Tin, Task<Tout>> localfunc;
        public Type ParmterType { get; set; }
        public LocalMethodDelegate(MethodInfo method, object instence)
        {
            localfunc = (Func<Tin, Task<Tout>>)method.CreateDelegate(typeof(Func<Tin, Task<Tout>>), instence);
            ParmterType = typeof(Tin);
        }
        public async Task<object> Excute(object val)
        {
            return await localfunc((Tin)val);
        }
    }
}
