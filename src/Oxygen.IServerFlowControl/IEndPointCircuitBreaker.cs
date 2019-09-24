using Oxygen.IServerFlowControl.Configure;
using System.Net;
using System.Threading.Tasks;

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
        Task<IPEndPoint> CheckCircuitByEndPoint(ServiceConfigureInfo configure, IPEndPoint clientEndPoint);
    }
}
