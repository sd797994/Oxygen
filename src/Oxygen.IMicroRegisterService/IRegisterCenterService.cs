using System.Net;
using System.Threading.Tasks;

namespace Oxygen.IMicroRegisterService
{
    public interface IRegisterCenterService
    {
        /// <summary>
        /// 服务注册
        /// </summary>
        /// <param name="localIp"></param>
        /// <param name="tcpPort"></param>
        /// <param name="serverName"></param>
        /// <returns></returns>
        Task<bool> RegisterService(IPAddress localIp, int tcpPort, string serverName);

        /// <summary>
        /// 服务注销
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        Task<bool> UnRegisterService();

        /// <summary>
        /// 服务发现
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        Task<IPEndPoint> GetServieByName(string serverName);
    }
}
