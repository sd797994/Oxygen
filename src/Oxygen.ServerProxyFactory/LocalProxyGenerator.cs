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
        private static readonly ConcurrentDictionary<string, ILocalMethodDelegate> InstanceDictionary = new ConcurrentDictionary<string, ILocalMethodDelegate>();
        public LocalProxyGenerator(IOxygenLogger logger, ISerialize serialize, CustomerInfo customerInfo)
        {
            _logger = logger;
            _serialize = serialize;
            _customerInfo = customerInfo;
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
                message.Message = await Task.FromResult(ExcutePath(message.Path, _serialize.Serializes(message.Message)));
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
        public object ExcutePath(string pathname, byte[] input)
        {
            if (InstanceDictionary.TryGetValue(pathname, out ILocalMethodDelegate methodDelegate))
            {
                return methodDelegate.Excute(_serialize.Deserializes(methodDelegate.ParmterType, input));
            }
            else
            {
                methodDelegate = CreateMethodDelegate(pathname);
                if(InstanceDictionary.TryAdd(pathname, methodDelegate))
                {
                    return methodDelegate.Excute(_serialize.Deserializes(methodDelegate.ParmterType, input));
                }
            }
            return null;
        }
        /// <summary>
        /// 创建本地方法委托
        /// </summary>
        /// <returns></returns>
        private ILocalMethodDelegate CreateMethodDelegate(string pathname)
        {
            var serviceName = pathname.Split('/')[0];
            var methodName = pathname.Split('/')[1];
            var type = RpcInterfaceType.Types.Value.AsEnumerable().FirstOrDefault(x=>x.Name.Contains(serviceName));
            var instance = OxygenIocContainer.Resolve(type);
            var method = type.GetMethod(methodName);
            return (ILocalMethodDelegate)Activator.CreateInstance(typeof(LocalMethodDelegate<,>).MakeGenericType(method.GetParameters()[0].ParameterType, method.ReturnType), method, instance);
        }
    }
}
