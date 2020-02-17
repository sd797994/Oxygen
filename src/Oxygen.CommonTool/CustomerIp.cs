using System;
using System.Collections.Generic;
using System.Net;

namespace Oxygen.CommonTool
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    public class CustomerInfo
    {
        /// <summary>
        /// 客户端IP
        /// </summary>
        public IPEndPoint Ip { get; set; }
        /// <summary>
        /// 客户端追踪http头
        /// </summary>
        public Dictionary<string, string> TraceHeaders { get; set; }
    }
}
