using Consul;
using Oxygen.IServerFlowControl;
using Oxygen.IServerRegisterManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.ConsulServerRegisterManage
{
    public class ConsulRegisterCenter : IRegisterCenter
    {
        private static string _clientId;
        public ConsulRegisterCenter()
        {
            _clientId = _clientId ?? Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 提供注册服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public async Task<bool> RegisterService(string serviceName, IPEndPoint iPEndPoint)
        {
            var result = await ConsulFactory.GetClient().Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = _clientId,
                Name = serviceName,
                Address = iPEndPoint.Address.ToString(),
                Port = iPEndPoint.Port,
                Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    Interval = TimeSpan.FromSeconds(10),
                    TCP = $"{iPEndPoint.Address}:{iPEndPoint.Port}",
                    Timeout = TimeSpan.FromSeconds(5)
                }
            });
            return result.StatusCode == System.Net.HttpStatusCode.OK;

        }

        /// <summary>
        /// 提供注销服务
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UnRegisterService()
        {
            var result = await ConsulFactory.GetClient().Agent.ServiceDeregister(_clientId);
            return result.StatusCode == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// 根据路由名称返回流控IP
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public async Task<List<FlowControlEndPoint>> GetServieByName(string serverName)
        {
            var remoteSrv = await ConsulFactory.GetClient().Agent.Services();
            var addrs = remoteSrv.Response.Values.Where(x => x.Service.ToLower().Equals(serverName.ToLower())).ToList();
            if (addrs.Any())
            {
                return addrs.Select(x => new FlowControlEndPoint(IPAddress.Parse(x.Address), x.Port)).ToList();
            }
            return default;
        }
    }
}
