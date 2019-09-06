using Consul;
using Oxygen.CommonTool;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.ConsulServerRegisterManage
{
    public class ConsulFactory
    {
        private static ConsulClient _client;
        public static ConsulClient GetClient()
        {
            _client = _client ?? new ConsulClient(ConfigurationOverview);
            return _client;
        }
        /// <summary>
        /// 返回consul配置节
        /// </summary>
        /// <param name="obj"></param>
        private static void ConfigurationOverview(ConsulClientConfiguration obj)
        {
            obj.Address = new Uri(OxygenSetting.ConsulAddress);
            obj.Datacenter = "dc1";
        }
    }
}
