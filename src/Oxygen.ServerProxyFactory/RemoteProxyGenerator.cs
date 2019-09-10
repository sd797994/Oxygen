using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
using Oxygen.IServerFlowControl;
using Oxygen.IServerProxyFactory;
using System;
using System.Net;
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
        private CustomerIp _customerIp;
        public RemoteProxyGenerator(IRpcClientProvider clientProvider, IOxygenLogger oxygenLogger
            , IFlowControlCenter flowControlCenter, IEndPointConfigureManager configureManager, CustomerIp customerIp)
        {
            _clientProvider = clientProvider;
            _oxygenLogger = oxygenLogger;
            _flowControlCenter = flowControlCenter;
            _customerIp = customerIp;
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
                //流量控制
                var ipendpoint = await _flowControlCenter.GetFlowControlEndPointByServicePath(serviceName, flowControlCfgKey, _customerIp.Ip);
                if (ipendpoint.endPoint != null)
                {
                    var channelKey = await _clientProvider.CreateClient(ipendpoint.endPoint, serviceName, pathName);
                    if (channelKey != null)
                    {
                        return await _clientProvider.SendMessage<TOut>(channelKey, ipendpoint.endPoint, flowControlCfgKey, ipendpoint.configureInfo, serviceName, pathName, input);
                    }
                    else
                    {
                        //强制熔断当前节点
                        if (ipendpoint.configureInfo != null)
                            _configureManager.ForcedCircuitBreakEndPoint(flowControlCfgKey, ipendpoint.configureInfo, ipendpoint.endPoint);
                        throw new Exception($"创建通道失败:{ipendpoint.ToString()}");
                    }
                }
            }
            catch (Exception e)
            {
                _oxygenLogger.LogError($"远程调用失败:{e.Message}");
            }
            return await Task.FromResult(default(TOut));
        }
    }
}
