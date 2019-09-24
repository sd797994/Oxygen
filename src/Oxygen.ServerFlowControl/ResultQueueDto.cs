using Oxygen.IServerFlowControl;
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Oxygen.ServerFlowControl
{
    /// <summary>
    /// RPC请求成功回调队列
    /// </summary>
    public class ResultQueueDto
    {
        public ResultQueueDto(string key, IPEndPoint endPoint, string flowControlCfgKey, ServiceConfigureInfo configure, object result)
        {
            Key = key;
            EndPoint = endPoint;
            FlowControlCfgKey = flowControlCfgKey;
            Configure = configure;
            Result = result;
        }
        public string Key { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public string FlowControlCfgKey { get; set; }
        public ServiceConfigureInfo Configure { get; set; }
        public object Result { get; set; }
    }
}
