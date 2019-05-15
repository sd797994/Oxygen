using Oxygen.CommonTool.Logger;
using Oxygen.IMicroRegisterService;
using Oxygen.IRpcProviderService;
using Oxygen.IServerProxyFactory;
using System;
using System.Threading.Tasks;

namespace Oxygen.ServerProxyFactory
{
    public class RemoteProxyGenerator: IRemoteProxyGenerator
    {
        private readonly IRegisterCenterService _registerCenterService;
        private readonly IRpcClientProvider _clientProvider;
        private readonly IOxygenLogger _oxygenLogger;
        public RemoteProxyGenerator(IRegisterCenterService registerCenterService, IRpcClientProvider clientProvider, IOxygenLogger oxygenLogger)
        {
            _registerCenterService = registerCenterService;
            _clientProvider = clientProvider;
            _oxygenLogger = oxygenLogger;
        }

        /// <summary>
        /// 通过代理类发送消息到远程服务器
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="input"></param>
        /// <param name="serviceName"></param>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public async Task<TOut> SendAsync<TIn, TOut>(TIn input, string serviceName, string pathName)
        {
            var remoteAddr = await _registerCenterService.GetServieByName(serviceName);
            if (remoteAddr == null)
            {
                _oxygenLogger.LogError($"注册中心没有找到有效的远程服务器[{serviceName}],调用失败");
            }
            else
            {
                try
                {
                    await _clientProvider.CreateClient(remoteAddr);
                    return await _clientProvider.SendMessage<TOut>(remoteAddr, pathName, input);
                }
                catch (Exception e)
                {
                    _oxygenLogger.LogError($"远程调用失败{e.Message}");
                }
            }
            return await Task.FromResult(default(TOut));
        }
    }
}
