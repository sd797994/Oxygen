using Consul;
using Oxygen.CommonTool;
using System;
using System.Collections.Concurrent;

namespace Oxygen.ConsulServerRegisterManage
{
    /// <summary>
    /// Consul客户端工厂
    /// </summary>
    public class ConsulFactory
    {
        private static readonly Lazy<ConcurrentDictionary<string, NodeCache>> _lazynodeCaches = new Lazy<ConcurrentDictionary<string, NodeCache>>(() => { return new ConcurrentDictionary<string, NodeCache>(); });
        private static readonly Lazy<ConsulClient> _lazyclient = new Lazy<ConsulClient>(() => { return new ConsulClient(ConfigurationOverview); });
        /// <summary>
        /// 创建并返回Consul客户端
        /// </summary>
        /// <returns></returns>
        public static ConsulClient GetClient()
        {
            return _lazyclient.Value;
        }
        /// <summary>
        /// 创建并返回Consul客户端缓存
        /// </summary>
        /// <returns></returns>
        public static ConcurrentDictionary<string, NodeCache> GetServiceCache()
        {
            return _lazynodeCaches.Value;
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
