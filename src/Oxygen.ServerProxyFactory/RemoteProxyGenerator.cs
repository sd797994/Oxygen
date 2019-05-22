using Oxygen.CommonTool.Logger;
using Oxygen.IRpcProviderService;
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
        public RemoteProxyGenerator(IRpcClientProvider clientProvider, IOxygenLogger oxygenLogger)
        {
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
            try
            {
                await _clientProvider.CreateClient(serviceName);
                return await _clientProvider.SendMessage<TOut>(serviceName, pathName, input);
            }
            catch (Exception e)
            {
                _oxygenLogger.LogError($"远程调用失败{e.Message}");
            }
            return await Task.FromResult(default(TOut));
        }
    }
}
