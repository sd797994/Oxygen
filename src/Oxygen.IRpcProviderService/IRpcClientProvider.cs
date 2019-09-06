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
        /// <param name="endPoint"></param>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<string> CreateClient(IPEndPoint endPoint, string serverName, string path);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channelKey"></param>
        /// <param name="endPoint"></param>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<T> SendMessage<T>(string channelKey, IPEndPoint endPoint, string key, string path, object message) where T : class;


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<object> SendMessage(string channelKey, IPEndPoint endPoint, string key, string path, object message);
    }
}
