using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
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
        private readonly IMediator _mediator;
        private static readonly Assembly MediatRAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => x.FullName.Contains("LocalClient.g"));
        private static readonly ConcurrentDictionary<string, Type> InstanceDictionary = new ConcurrentDictionary<string, Type>();
        private readonly IOxygenLogger _logger;
        private readonly ISerialize _serialize;
        private readonly CustomerInfo _customerInfo;
        public LocalProxyGenerator(IMediator mediator, IOxygenLogger logger, ISerialize serialize, CustomerInfo customerInfo)
        {
            _mediator = mediator;
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
                _logger.LogError($"订阅者消息分发不能为空消息");
                return default;
            }
            else
            {
                if (!message.CheckSign(GlobalCommon.SHA256Encrypt(message.TaskId + OxygenSetting.SignKey)))
                {
                    _logger.LogError($"验签失败,任务ID:{message.TaskId}");
                    message.code = System.Net.HttpStatusCode.Unauthorized;
                    return message;
                }
                if (!InstanceDictionary.TryGetValue(message.Path, out var messageType))
                {
                    _customerInfo.Ip = message.CustomerIp;
                    messageType = MediatRAssembly.GetType($"Oxygen.MediatRProxyClientBuilder.ProxyInstance.{message.Path}");
                    if (messageType != null)
                    {
                        InstanceDictionary.TryAdd(message.Path, messageType);
                    }
                }
                if (messageType != null)
                {
                    message.Message = await Publish(_serialize.Deserializes(messageType, _serialize.Serializes(message.Message)));
                    message.code = System.Net.HttpStatusCode.OK;
                    return message;
                }
                else
                {
                    _logger.LogError($"未找到订阅者实例:{message.Path}");
                    message.code = System.Net.HttpStatusCode.NotFound;
                    return message;
                }
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<dynamic> Publish(dynamic message)
        {
            return await _mediator.Send(message);
        }
    }
}
