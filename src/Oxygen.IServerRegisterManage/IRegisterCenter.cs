using Oxygen.IServerFlowControl;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.IServerRegisterManage
{
    public interface IRegisterCenter
    {
        /// <summary>
        /// 提供注册服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        Task<bool> RegisterService(string serviceName, IPEndPoint iPEndPoint);

        /// <summary>
        /// 提供注销服务
        /// </summary>
        /// <returns></returns>
        Task<bool> UnRegisterService();

        /// <summary>
        /// 根据路由名称返回流控IP
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        Task<List<FlowControlEndPoint>> GetServieByName(string serverName);
    }
}
