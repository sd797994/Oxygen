using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oxygen.ProxyClientBuilder
{
    /// <summary>
    /// 远程代理构造类
    /// </summary>
    public class RemoteProxyClientBuilder
    {
        /// <summary>
        /// 预编译类型
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static List<StringBuilder> GetBodyFromType(Type[] types)
        {
            var bodyType = new List<StringBuilder>();
            foreach (var type in types)
            {
                var basenamespaces = new List<string>();

                var content = new StringBuilder();
                type.GetMethods().ToList().ForEach(method =>
                {
                    var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
                    if (!basenamespaces.Any(x => x.Equals(parameterType.Namespace)))
                    {
                        basenamespaces.Add(parameterType.Namespace);
                    }
                    var returnType = method.ReturnType;
                    if (returnType.IsGenericType)
                    {
                        returnType = returnType.GenericTypeArguments.FirstOrDefault();
                    }
                    if (!basenamespaces.Any(x => x.Equals(returnType.Namespace)))
                    {
                        basenamespaces.Add(returnType.Namespace);
                    }
                });
                basenamespaces.ForEach(x =>
                {
                    content.AppendLine($"using {x};");
                });
                var className = $"{type.Name.Substring(1, type.Name.Length - 1)}_ProxyClient";
                var serviceName = typeof(RemoteServiceAttribute).GetProperty("ServerName")
                    ?.GetValue(type.GetCustomAttribute(typeof(RemoteServiceAttribute)));
                content.AppendLine("using Oxygen.IServerProxyFactory;");
                content.AppendLine("using System.Threading.Tasks;");
                content.AppendLine("namespace Oxygen.RemoteProxyClientBuilder.ProxyInstance");
                content.AppendLine("{");
                content.AppendLine($"    public class {className} : {type.Name}");
                content.AppendLine("    {");
                content.AppendLine("        private readonly IRemoteProxyGenerator _proxyGenerator;");
                content.AppendLine($"        public {className}(IRemoteProxyGenerator proxyGenerator)");
                content.AppendLine("        {");
                content.AppendLine("            _proxyGenerator = proxyGenerator;");
                content.AppendLine("        }");
                type.GetMethods().ToList().ForEach(method =>
                {
                    var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
                    var returnType = method.ReturnType;
                    if (returnType.IsGenericType)
                    {
                        returnType = returnType.GenericTypeArguments.FirstOrDefault();
                    }
                    var pathName = $"{type.Name.Substring(1, type.Name.Length - 1)}_{parameterType.Name}";
                    content.AppendLine($"        public async Task<{returnType.Name}> {method.Name}({parameterType.Name} input)");
                    content.AppendLine("        {");
                    content.AppendLine($"            return await _proxyGenerator.SendAsync<{parameterType.Name}, {returnType.Name}>(input,\"{serviceName}\",\"{pathName}\");");
                    content.AppendLine("        }");
                });
                content.AppendLine("    }");
                content.AppendLine("}");
                bodyType.Add(content);
            }
            return bodyType;
        }
    }
}
