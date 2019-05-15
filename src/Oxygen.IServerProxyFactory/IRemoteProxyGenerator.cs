using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    public interface IRemoteProxyGenerator
    {
        /// <summary>
        /// 通过代理类发送消息到远程服务器
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="input"></param>
        /// <param name="serviceName"></param>
        /// <param name="pathName"></param>
        /// <returns></returns>
        Task<TOut> SendAsync<TIn, TOut>(TIn input, string serviceName, string pathName);
    }
}
