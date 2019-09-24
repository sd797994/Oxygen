using Consul;
using Nito.AsyncEx;
using Oxygen.IServerRegisterManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Oxygen.ConsulServerRegisterManage
{
    /// <summary>
    /// Consul注册中心
    /// </summary>
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
        public async Task<List<IPEndPoint>> GetServieByName(string serverName)
        {
            bool NeedFlush = false;
            if (ConsulFactory.GetServiceCache().TryGetValue(serverName, out NodeCache value))
            {
                if (value.ExpirTime.AddSeconds(10) > DateTime.Now)
                {
                    return value.AgentServices.Select(x => new IPEndPoint(IPAddress.Parse(x.Address), x.Port)).ToList();
                }
                else
                {
                    ConsulFactory.GetServiceCache().TryRemove(serverName, out NodeCache removCache);
                    NeedFlush = true;
                }
            }
            else
            {
                NeedFlush = true;
            }
            if (NeedFlush)
            {
                var remoteSrv = await ConsulFactory.GetClient().Agent.Services();
                var addrs = remoteSrv.Response.Values.Where(x => x.Service.ToLower().Equals(serverName.ToLower())).ToList();
                if (addrs.Any())
                {
                    ConsulFactory.GetServiceCache().TryAdd(serverName, new NodeCache() { ExpirTime = DateTime.Now, AgentServices = addrs });
                    return addrs.Select(x => new IPEndPoint(IPAddress.Parse(x.Address), x.Port)).ToList();
                }
            }
            return default;
        }
    }
}
