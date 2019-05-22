using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    /// <summary>
    /// 虚拟代理接口
    /// </summary>
    public interface IVirtualProxyServer
    {
        /// <summary>
        /// 通过虚拟代理发送请求
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<object> SendAsync(object input);
    }
}
