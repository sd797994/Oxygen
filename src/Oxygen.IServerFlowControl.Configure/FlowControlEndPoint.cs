using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Oxygen.IServerFlowControl.Configure
{
    /// <summary>
    /// 流控服务IP类
    /// </summary>
    public class FlowControlEndPoint
    {
        public FlowControlEndPoint() { }
        public FlowControlEndPoint(IPEndPoint point)
        {
            this.Address = point.Address.ToString();
            this.Port = point.Port;
        }
        public FlowControlEndPoint(string address, int port)
        {
            this.Address = address;
            this.Port = port;
        }
        public FlowControlEndPoint(IPAddress address, int port)
        {
            this.Address = address.ToString();
            this.Port = port;
        }
        public IPEndPoint GetEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(Address), Port);
        }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 哈希排序
        /// </summary>
        public int HashSort { get; set; }

        /// <summary>
        /// 连接数
        /// </summary>
        public int ConnectCount { get; set; }

        /// <summary>
        /// 上次熔断时间
        /// </summary>
        public DateTime? BreakerTime { get; set; }

        /// <summary>
        /// 已重试熔断次数，恢复后清零
        /// </summary>
        public int ThresholdBreakeTimes { get; set; }
    }
    /// <summary>
    /// 流控endpoint扩展
    /// </summary>
    public static class IPEndPointExtension
    {
        public static FlowControlEndPoint GetFlowControlEndPoint(this IPEndPoint endpoint)
        {
            return new FlowControlEndPoint(endpoint);
        }

        public static List<FlowControlEndPoint> GetFlowControlEndPoints(this List<IPEndPoint> endpoints)
        {
            return endpoints.Select(endpoint => new FlowControlEndPoint(endpoint)).ToList();
        }
    }

}
