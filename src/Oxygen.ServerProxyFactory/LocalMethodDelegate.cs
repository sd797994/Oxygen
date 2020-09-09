using Oxygen.IProxyClientBuilder;
using Oxygen.IServerProxyFactory;
using Oxygen.ServerProxyFactory;
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
    public class LocalMethodDelegate<Tin, Tout> : ILocalMethodDelegate where Tout : class
    {
        private Func<Tin, Task<Tout>> localfunc;
        public Type Type { get; set; }
        public Type ParmterType { get; set; }
        public MethodInfo Method { get; set; }
        public LocalMethodDelegate(MethodInfo method, Type type)
        {
            Method = method;
            Type = type;
            ParmterType = typeof(Tin);
        }
        public void Build(object obj)
        {
            localfunc = (Func<Tin, Task<Tout>>)Method.CreateDelegate(typeof(Func<Tin, Task<Tout>>), obj);
        }
        public async Task<object> Excute(object val)
        {
            return await LocalMethodAopProvider.UsePipelineHandler((Tin)val, localfunc);
        }
    }
}
