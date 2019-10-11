using Oxygen.IRpcProviderService;
using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    /// <summary>
    /// 本地代理消息分发处理接口
    /// </summary>
    public interface ILocalProxyGenerator
    {
        /// <summary>
        /// 消息分发处理
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<RpcGlobalMessageBase<object>> Invoke(RpcGlobalMessageBase<object> message);
    }
}
