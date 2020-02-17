using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    /// <summary>
    /// 远程代理服务生成器
    /// </summary>
    public interface IRemoteProxyGenerator
    {
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
        Task<TOut> SendAsync<TIn, TOut>(TIn input, Dictionary<string, string> traceHeaders, string serviceName, string pathName) where TOut : class;
        Task<object> SendObjAsync<TIn>(TIn input, Type OutType, string serviceName, string pathName);
    }
}
