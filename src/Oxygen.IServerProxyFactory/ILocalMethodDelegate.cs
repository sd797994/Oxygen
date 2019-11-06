using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.IServerProxyFactory
{
    public interface ILocalMethodDelegate
    {
        Type ParmterType { get; set; }
        object Excute(object val);
    }
}
