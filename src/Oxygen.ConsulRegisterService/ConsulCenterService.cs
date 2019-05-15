using Consul;
using Oxygen.Common;
using Oxygen.IMicroRegisterService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Oxygen.ConsulRegisterService
{
    public class ConsulCenterService : IRegisterCenterService
    {
        private readonly IGlobalCommon _globalCommon;
        private static Dictionary<string, AgentService> _localCache = new Dictionary<string, AgentService>();
        public ConsulCenterService(IGlobalCommon globalCommon)
        {
            _globalCommon = globalCommon;
        }
        void ConfigurationOverview(ConsulClientConfiguration obj)
        {
            obj.Address = new Uri($"http://{OxygenSetting.Consul}");
            obj.Datacenter = "dc1";
        }

        private static ConsulClient _client;
        private static string ClientId { get; set; }
        public ConsulClient GetClient()
        {
            _client = _client ?? new ConsulClient(ConfigurationOverview);
            return _client;
        }

        /// <summary>
        /// 服务注册
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RegisterService(IPAddress localIp, int tcpPort, string serverName)
        {
            ClientId = ClientId ?? Guid.NewGuid().ToString();
            await GetClient().Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = ClientId,
                Name = serverName,
                Address = localIp.ToString(),
                Port = tcpPort,
                Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    Interval = TimeSpan.FromSeconds(10),
                    TCP = $"{localIp}:{tcpPort}",
                    Timeout = TimeSpan.FromSeconds(5)
                }
            });
            return await Task.FromResult(true);
        }

        /// <summary>
        /// 服务注销
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UnRegisterService()
        {
            await GetClient().Agent.ServiceDeregister(ClientId);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// 服务发现
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public async Task<IPEndPoint> GetServieByName(string serverName)
        {
            if (_localCache == null || !_localCache.Any())
            {
                _localCache = (await GetClient().Agent.Services()).Response;
            }
            var addrs = _localCache.Values.Where(x => x.Service.Equals(serverName));
            var random = new Random(Guid.NewGuid().GetHashCode());
            if (addrs.Any())
            {
                var services = addrs.ToArray()[random.Next(0, addrs.Count())];
                return new IPEndPoint(IPAddress.Parse(services.Address), services.Port);
            }
            return await Task.FromResult(default(IPEndPoint));
        }
    }
}
