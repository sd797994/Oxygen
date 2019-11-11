using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    public interface ILocalMethodDelegate
    {
        Type ParmterType { get; set; }
        Task<object> Excute(object val);
    }
}
