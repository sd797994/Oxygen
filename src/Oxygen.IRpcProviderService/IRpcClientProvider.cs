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
        /// <returns></returns>
        Task CreateClient(EndPoint endPoint);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endPoint"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<T> SendMessage<T>(EndPoint endPoint, string path, object message);


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<object> SendMessage(EndPoint endPoint, string path, object message);
    }
}
