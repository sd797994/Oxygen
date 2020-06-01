using System;
using System.Net;
using System.Threading.Tasks;

namespace Oxygen.IRpcProviderService
{
    /// <summary>
    /// 服务端消息服务接口
    /// </summary>
    public interface IRpcServerProvider
    {
        /// <summary>
        /// 启动tcp服务
        /// </summary>
        /// <returns></returns>
        Task<IPEndPoint> OpenServer(Action<object> middlewarebuilder = null);

        /// <summary>
        /// 关闭tcp服务
        /// </summary>
        /// <returns></returns>
        Task CloseServer();
    }
}
