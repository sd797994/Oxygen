using Microsoft.Extensions.DependencyModel;
using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Oxygen.CommonTool
{
    public class RpcInterfaceType
    {
        public static Lazy<IEnumerable<Type>> Types = new Lazy<IEnumerable<Type>>(() =>
        {
            var list = new List<Assembly>();
            var deps = DependencyContext.Default;
            var libs = deps.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type != "package");//排除所有的系统程序集、Nuget下载包
            foreach (var lib in libs)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
                    list.Add(assembly);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return list.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute)).Any() && t.IsInterface)).ToArray();
        });
    }
}
