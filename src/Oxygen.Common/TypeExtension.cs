using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oxygen.Common
{
    public static class TypeExtension
    {
        /// <summary>
        /// 通过compilation预编译程序集
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assemblyName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IEnumerable<Type> BuildType(this Type[] type, string assemblyName,
            IEnumerable<StringBuilder> content)
        {

            var syntaxTree = content.Select(x => CSharpSyntaxTree.ParseText(x.ToString()));
            var compilation = CSharpCompilation.Create(assemblyName, syntaxTree,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                    .Select(x => MetadataReference.CreateFromFile(x.Location)));
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    return Assembly.Load(ms.ToArray()).GetTypes();
                }
                throw new Exception(string.Join(",", result.Diagnostics));
            }
        }
    }
}
