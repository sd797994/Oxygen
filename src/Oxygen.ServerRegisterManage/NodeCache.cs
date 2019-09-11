using Consul;
using System;
using System.Collections.Generic;

namespace Oxygen.ConsulServerRegisterManage
{
    /// <summary>
    /// Consul客户端缓存类
    /// </summary>
    public class NodeCache
    {
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpirTime { get; set; }
        /// <summary>
        /// 代理类集合
        /// </summary>
        public List<AgentService> AgentServices { get; set; }
    }
}
