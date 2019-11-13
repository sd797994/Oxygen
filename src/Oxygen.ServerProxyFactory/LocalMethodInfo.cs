using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Oxygen.ServerProxyFactory
{
    public class LocalMethodInfo
    {
        public Type Type { get; set; }
        public MethodInfo Method { get; set; }
        public Type ParameterType { get; set; }
        public Type ReturnType { get; set; }
    }
}
