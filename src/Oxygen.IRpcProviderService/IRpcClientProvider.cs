using System.Net;
using System.Threading.Tasks;

namespace Oxygen.IRpcProviderService
{
    /// <summary>
    /// 客户端消息服务接口
    /// </summary>
    public interface IRpcClientProvider
    {
        /// <summary>
        /// 创建客户端实例
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        Task CreateClient(string serverName);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<T> SendMessage<T>(string serverName, string path, object message);


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<object> SendMessage(string serverName, string path, object message);
    }
}
