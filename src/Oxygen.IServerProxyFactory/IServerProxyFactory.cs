using System.Threading.Tasks;

namespace Oxygen.IServerProxyFactory
{
    /// <summary>
    /// 服务代理工厂接口
    /// </summary>
    public interface IServerProxyFactory
    {
        /// <summary>
        /// 通过强类型创建代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> CreateProxy<T>() where T : class;

        /// <summary>
        /// 通过路径创建代理
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IVirtualProxyServer> CreateProxy(string path);
    }
}
