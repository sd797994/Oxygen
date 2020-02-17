using Autofac;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.IServerProxyFactory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oxygen.ServerProxyFactory
{
    /// <summary>
    /// 远程代理服务生成器
    /// </summary>
    public class RemoteProxyGenerator: IRemoteProxyGenerator
    {
        private readonly IRpcClientProvider _clientProvider;
        private readonly IOxygenLogger _oxygenLogger;
        private readonly CustomerInfo customerInfo;
        public RemoteProxyGenerator(IRpcClientProvider clientProvider, IOxygenLogger oxygenLogger, CustomerInfo customerInfo)
        {
            _clientProvider = clientProvider;
            _oxygenLogger = oxygenLogger;
            this.customerInfo = customerInfo;
        }

        /// <summary>
        /// 通过代理类发送消息到远程服务器
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="input"></param>
        /// <param name="serviceName"></param>
        /// <param name="FlowControlCfgKey"></param>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public async Task<TOut> SendAsync<TIn, TOut>(TIn input, Dictionary<string, string> traceHeaders, string serviceName, string pathName) where TOut : class
        {
            return await SendAsync<TIn, TOut>(input, serviceName, pathName, null, traceHeaders);
        }
        public async Task<object> SendObjAsync<TIn>(TIn input, Type OutType, string serviceName, string pathName)
        {
            return await SendAsync<TIn, object>(input, serviceName, pathName, OutType, null);
        }
        private async Task<TOut> SendAsync<TIn, TOut>(TIn input, string serviceName, string pathName, Type OutType = null, Dictionary<string, string> traceHeaders = null) where TOut : class
        {
            try
            {
                if (await _clientProvider.CreateClient(serviceName))
                {
                    if (OutType == null)
                        return await _clientProvider.SendMessage<TOut>(serviceName, pathName, input, traceHeaders ?? customerInfo.TraceHeaders);
                    else
                        return await _clientProvider.SendMessage(serviceName, pathName, input, OutType, traceHeaders ?? customerInfo.TraceHeaders) as TOut;
                }
                else
                {
                    _oxygenLogger.LogError($"远程调用通道创建失败,服务名:{serviceName}");
                }
            }
            catch (Exception e)
            {
                _oxygenLogger.LogError($"远程调用失败:{e.Message},堆栈跟踪:{e.StackTrace.ToString()}");
            }
            return await Task.FromResult(default(TOut));
        }
    }
}
