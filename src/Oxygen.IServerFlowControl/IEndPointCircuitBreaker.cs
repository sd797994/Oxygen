using System.Net;

namespace Oxygen.IServerFlowControl
{
    /// <summary>
    /// 断路器
    /// </summary>
    public interface IEndPointCircuitBreaker
    {
        /// <summary>
        /// 检查服务断路状态
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="serviceInfo"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        bool CheckCircuitByEndPoint(string pathName, IPEndPoint clientEndPoint, ServiceConfigureInfo serviceInfo, out IPEndPoint addr);
    }
}
