using Microsoft.Extensions.DependencyModel;
using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using Oxygen.ICache;
using Oxygen.IServerFlowControl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;

namespace Oxygen.ServerFlowControl
{
    /// <summary>
    /// 令牌桶
    /// </summary>
    public class TokenBucket: ITokenBucket
    {
        private readonly IEndPointConfigureManager _endPointConfigureManager;

        public TokenBucket(IEndPointConfigureManager endPointConfigureManager)
        {
            _endPointConfigureManager = endPointConfigureManager;
        }
        /// <summary>
        /// 初始化桶
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="rate"></param>
        public void InitTokenBucket(long capacity, long rate)
        {
            this.Capacity = capacity;
            this.Rate = TimeSpan.FromMilliseconds(rate * 1000).Ticks; ;
        }
        /// <summary>
        /// 桶的大小
        /// </summary>
        private long Capacity { get; set; }
        /// <summary>
        /// 桶的流速
        /// </summary>
        private long Rate { get; set; }
        /// <summary>
        /// 检查令牌是否可用
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public bool Grant(string key, ServiceConfigureInfo serviceInfo)
        {
            var bucketInfo = _endPointConfigureManager.GetOrAddTokenBucket(key, serviceInfo);
            _endPointConfigureManager.UpdateTokens(bucketInfo, Capacity, Rate);
            if (bucketInfo.Tokens < 1)
            {
                var timeToIntervalEnd = bucketInfo.StartTimeStamp - DateTime.UtcNow.Ticks;
                if (timeToIntervalEnd < 0) return Grant(key, serviceInfo);
                return false;
            }
            bucketInfo.Tokens -= 1;
            _endPointConfigureManager.UpdateTokenBucket(key, bucketInfo);
            return true;
        }
    }
}
