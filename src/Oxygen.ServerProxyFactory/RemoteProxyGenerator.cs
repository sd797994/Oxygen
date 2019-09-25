using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.IServerFlowControl;
using Oxygen.IServerFlowControl.Configure;
using Oxygen.IServerProxyFactory;
using System;
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
        private readonly IFlowControlCenter _flowControlCenter;
        private readonly IEndPointConfigureManager _configureManager;
        private readonly CustomerInfo _customerInfo;
        public RemoteProxyGenerator(IRpcClientProvider clientProvider, IOxygenLogger oxygenLogger
            , IFlowControlCenter flowControlCenter, IEndPointConfigureManager configureManager, CustomerInfo customerInfo)
        {
            _clientProvider = clientProvider;
            _oxygenLogger = oxygenLogger;
            _flowControlCenter = flowControlCenter;
            _customerInfo = customerInfo;
            _configureManager = configureManager;
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
        public async Task<TOut> SendAsync<TIn, TOut>(TIn input, string serviceName, string flowControlCfgKey, string pathName) where TOut : class
        {
            try
            {
                var configure = await _configureManager.GetBreakerConfigure(flowControlCfgKey);
                //流量控制
                var ipendpoint = await _flowControlCenter.GetFlowControlEndPointByServicePath(serviceName, configure, _customerInfo.Ip);
                if (ipendpoint != null)
                {
                    var channelKey = await _clientProvider.CreateClient(ipendpoint, flowControlCfgKey);
                    if (channelKey != null)
                    {
                        return await _clientProvider.SendMessage<TOut>(channelKey, ipendpoint, flowControlCfgKey, configure, serviceName, pathName, input);
                    }
                    else
                    {
                        _oxygenLogger.LogError($"远程调用通道创建失败:{ipendpoint.ToString()}");
                        //强制熔断当前节点
                        await _configureManager.ForcedCircuitBreakEndPoint(flowControlCfgKey, configure, ipendpoint);
                    }
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
