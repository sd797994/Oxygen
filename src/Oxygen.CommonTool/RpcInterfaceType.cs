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
        public static Lazy<IEnumerable<Type>> RemoteTypes = new Lazy<IEnumerable<Type>>(() =>
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
            var interfaces = list.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute)).Any() && t.IsInterface)).ToArray();
            var localclass = list.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any() && interfaces.Contains(t.GetInterfaces().FirstOrDefault()))).Select(x => x.GetInterfaces().FirstOrDefault()).ToList();
            return interfaces.Where(x => !localclass.Contains(x));
        });
        public static Lazy<IEnumerable<Type>> LocalTypes = new Lazy<IEnumerable<Type>>(() =>
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
            var interfaces = list.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(RemoteServiceAttribute)).Any() && t.IsInterface)).ToArray();
            return list.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any() && interfaces.Contains(t.GetInterfaces().FirstOrDefault()))).Select(x => x.GetInterfaces().FirstOrDefault()).ToList();
        });


        public static Lazy<IEnumerable<(Type interfaceType, Type? classType, bool autoSave)>> ActorTypes = new Lazy<IEnumerable<(Type interfaceType, Type? classType, bool autoSave)>>(() =>
        {
            var result = new List<(Type interfaceType, Type? classType, bool autoSave)>();
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
            var interfaces = list.SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(ActorServiceAttribute)).Any() && t.IsInterface)).ToList();
            var actortypes = list.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any() && interfaces.Contains(t.GetInterfaces().FirstOrDefault()))).ToList();
            if (interfaces.Any())
            {
                interfaces.ForEach(x =>
                {
                    Type? _class = actortypes.FirstOrDefault(y => y.GetInterface(x.Name) != null) ?? null;
                    var autosave = x.GetCustomAttribute<ActorServiceAttribute>().AutoSave;
                    result.Add((x, _class, autosave));
                });
            }
            return result;
        });
    }
}
