using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxygen.ProxyClientBuilder
{
    public static class LocalProxyClientBuilder
    {
        public static List<StringBuilder> GetBodyFromType(Type[] types)
        {
            var bodyType = new List<StringBuilder>();
            foreach (var type in types)
            {
                var basenamespaces = new List<string>();
                var baseInterface = type.GetInterfaces().FirstOrDefault();
                basenamespaces.Add(baseInterface.Namespace);
                var className = type.Name;
                type.GetMethods().Where(x => x.IsFinal).ToList().ForEach(method =>
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
                    var content = new StringBuilder();
                    basenamespaces.ForEach(x =>
                    {
                        content.AppendLine($"using {x};");
                    });
                    content.AppendLine("using MediatR;");
                    content.AppendLine("using Oxygen.Common;");
                    content.AppendLine("using System.Threading;");
                    content.AppendLine("using System.Threading.Tasks;");
                    content.AppendLine($"namespace Oxygen.MediatRProxyClientBuilder.ProxyInstance");
                    content.AppendLine("{");
                    content.AppendLine($"	public class {className}_{parameterType.Name} : IRequest<{returnType.Name}>");
                    content.AppendLine("	{");
                    foreach (var propertyInfo in parameterType.GetProperties())
                    {
                        content.AppendLine($"        public {propertyInfo.PropertyType} {propertyInfo.Name} {{ get; set; }}");
                    }
                    content.AppendLine("	}");
                    content.AppendLine($"	public class {className}_Local_{parameterType.Name} : IRequestHandler<{className}_{parameterType.Name},{returnType.Name}>");
                    content.AppendLine("	{");
                    content.AppendLine($"		private readonly {baseInterface.Name} _subscriber;");
                    content.AppendLine($"		public {className}_Local_{parameterType.Name}({baseInterface.Name} subscriber)");
                    content.AppendLine("		{");
                    content.AppendLine("			_subscriber = subscriber;");
                    content.AppendLine("		}");
                    content.AppendLine($"		public async Task<{returnType.Name}> Handle({className}_{parameterType.Name} input, CancellationToken cancellationToken)");
                    content.AppendLine("		{");
                    content.AppendLine($"			return await _subscriber.{method.Name}(Mapper<Oxygen.MediatRProxyClientBuilder.ProxyInstance.{className}_{parameterType.Name}, {parameterType.Namespace}.{parameterType.Name}>.Map(input));");
                    content.AppendLine("		}");
                    content.AppendLine("	}");
                    content.AppendLine("}");
                    bodyType.Add(content);
                });
            }
            return bodyType;
        }
    }
}
