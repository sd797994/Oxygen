using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Oxygen.ProxyClientBuilder
{
    public class LocalProxyClientBase : DynamicObject
    {
        public LocalProxyClientBase()
        {

        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            return true;
        }
    }
}
