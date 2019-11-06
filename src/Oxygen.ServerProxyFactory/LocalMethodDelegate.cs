using Oxygen.IProxyClientBuilder;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Oxygen.IProxyClientBuilder
{
    /// <summary>
    /// 代理委托
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public class LocalMethodDelegate<Tin, Tout> : ILocalMethodDelegate
    {
        private Func<Tin, Tout> localfunc;
        public Type ParmterType { get; set; }
        public LocalMethodDelegate(MethodInfo method, object instence)
        {
            localfunc = (Func<Tin, Tout>)method.CreateDelegate(typeof(Func<Tin, Tout>), instence);
            ParmterType = typeof(Tin);
        }
        public object Excute(object val)
        {
            return localfunc((Tin)val);
        }
    }
}
