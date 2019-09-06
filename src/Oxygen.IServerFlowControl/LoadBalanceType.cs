using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.IServerFlowControl
{
    public enum LoadBalanceType
    {
        /// <summary>
        /// 轮询
        /// </summary>
        Polling = 0,
        /// <summary>
        /// 随机
        /// </summary>
        Random = 1,
        /// <summary>
        /// IP哈希
        /// </summary>
        IPHash = 2,
        /// <summary>
        /// 最小连接
        /// </summary>
        MinConnections = 3
    }
}
