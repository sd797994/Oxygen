using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyModel;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.CsharpClientAgent;
using Oxygen.IProxyClientBuilder;
using Oxygen.IRpcProviderService;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 本地代理消息分发处理类
    /// </summary>
    public class LocalProxyGenerator : ILocalProxyGenerator
    {
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        private readonly ILifetimeScope container;
        private static readonly ConcurrentDictionary<string, ILocalMethodDelegate> InstanceDictionary = new ConcurrentDictionary<string, ILocalMethodDelegate>();
        public LocalProxyGenerator(IOxygenLogger logger, ISerialize serialize, CustomerInfo customerInfo, ILifetimeScope container)
        {
            _logger = logger;
            _serialize = serialize;
            this.container = container;
        }


        /// <summary>
        /// 消息分发处理
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<RpcGlobalMessageBase<object>> Invoke(RpcGlobalMessageBase<object> message)
        {
            if (message == null)
            {
                _logger.LogError($"消息分发不能为空消息");
                return default;
            }
            else
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    message.Message = await ExcutePath(message.Path, _serialize.Serializes(message.Message), scope);
                }
                message.code = System.Net.HttpStatusCode.OK;
                return message;
            }
        }

        /// <summary>
        /// 执行本地方法
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<object> ExcutePath(string pathname, byte[] input, ILifetimeScope lifetimeScope)
        {
            if (InstanceDictionary.TryGetValue(pathname, out ILocalMethodDelegate methodDelegate))
            {
                methodDelegate.Build(lifetimeScope.Resolve(methodDelegate.Type));
                return await methodDelegate.Excute(_serialize.Deserializes(methodDelegate.ParmterType, input));
            }
            return null;
        }
        /// <summary>
        /// 缓存本地方法类型信息
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public static void LoadMethodDelegate()
        {
            foreach (var type in RpcInterfaceType.LocalTypes.Value)
            {
                foreach(var method in type.GetMethods())
                {
                    var delegateObj = CreateMethodDelegate(type, method, out string key);
                    InstanceDictionary.TryAdd(key, delegateObj);
                }
            }
        }

        /// <summary>
        /// 创建本地方法委托
        /// </summary>
        /// <returns></returns>
        private static ILocalMethodDelegate CreateMethodDelegate(Type localType,MethodInfo method,out string pathname)
        {
            pathname = localType.Name + "/" + method.Name;
            return (ILocalMethodDelegate)Activator.CreateInstance(typeof(LocalMethodDelegate<,>).MakeGenericType(method.GetParameters()[0].ParameterType, method.ReturnType.GetGenericArguments().FirstOrDefault()), method, localType);
        }
    }
}
