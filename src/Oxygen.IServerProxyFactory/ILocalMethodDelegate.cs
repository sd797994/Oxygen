using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    public interface ILocalMethodDelegate
    {
        Type Type { get; set; }
        Type ParmterType { get; set; }
        void Build(object obj);
        Task<object> Excute(object val);
    }
}
