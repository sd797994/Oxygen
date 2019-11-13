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
        private readonly CustomerInfo _customerInfo;
        private readonly ILifetimeScope container;
        private static readonly ConcurrentDictionary<string, LocalMethodInfo> InstanceDictionary = new ConcurrentDictionary<string, LocalMethodInfo>();
        public LocalProxyGenerator(IOxygenLogger logger, ISerialize serialize, CustomerInfo customerInfo, ILifetimeScope container)
        {
            _logger = logger;
            _serialize = serialize;
            _customerInfo = customerInfo;
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
            if (InstanceDictionary.TryGetValue(pathname, out LocalMethodInfo methodInfo))
            {
                return await Build(methodInfo, lifetimeScope).Excute(_serialize.Deserializes(methodInfo.ParameterType, input));
            }
            else
            {
                methodInfo = CreateLocalMethodInfo(pathname);
                if (InstanceDictionary.TryAdd(pathname, methodInfo))
                {
                    return await Build(methodInfo, lifetimeScope).Excute(_serialize.Deserializes(methodInfo.ParameterType, input));
                }
            }
            return null;
        }

        /// <summary>
        /// 缓存本地方法类型信息
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private LocalMethodInfo CreateLocalMethodInfo(string pathname)
        {
            var serviceName = pathname.Split('/')[0];
            var methodName = pathname.Split('/')[1];
            var type = RpcInterfaceType.Types.Value.AsEnumerable().FirstOrDefault(x => x.Name.Contains(serviceName));
            var method = type.GetMethod(methodName);
            return new LocalMethodInfo() { Type = type, Method = type.GetMethod(methodName), ParameterType = method.GetParameters()[0].ParameterType, ReturnType = method.ReturnType.GetGenericArguments().FirstOrDefault() };
        }

        /// <summary>
        /// 创建本地方法委托
        /// </summary>
        /// <returns></returns>
        static Type delegateType = typeof(LocalMethodDelegate<,>);
        private ILocalMethodDelegate Build(LocalMethodInfo methodInfo, ILifetimeScope lifetimeScope)
        {
            return ((ILocalMethodDelegate)Activator.CreateInstance(delegateType.MakeGenericType(methodInfo.ParameterType, methodInfo.ReturnType), methodInfo.Method, lifetimeScope.Resolve(methodInfo.Type)));
        }
    }
}
