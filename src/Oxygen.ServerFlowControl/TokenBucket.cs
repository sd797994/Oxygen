using Microsoft.Extensions.DependencyModel;
using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using Oxygen.ICache;
using Oxygen.IServerFlowControl;
using Oxygen.IServerFlowControl.Configure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oxygen.ServerFlowControl
{
    /// <summary>
    /// 令牌桶
    /// </summary>
    public class TokenBucket: ITokenBucket
    {
        private readonly IEndPointConfigureManager _endPointConfigure;

        public TokenBucket(IEndPointConfigureManager endPointConfigure)
        {
            _endPointConfigure = endPointConfigure;
        }
        /// <summary>
        /// 初始化桶
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="rate"></param>
        public void InitTokenBucket(long capacity, long rate)
        {
            this.Capacity = capacity;
            this.Rate = TimeSpan.FromMilliseconds(rate * 1000).Ticks;
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
        public async Task<bool> Grant(string flowControlCfgKey, int defCapacity)
        {
            var bucketInfo = await _endPointConfigure.GetOrAddTokenBucket(flowControlCfgKey, defCapacity);
            _endPointConfigure.UpdateTokens(bucketInfo, Capacity, Rate);
            if (bucketInfo.Tokens < 1)
            {
                var timeToIntervalEnd = bucketInfo.StartTimeStamp - DateTime.UtcNow.Ticks;
                if (timeToIntervalEnd < 0) return await Grant(flowControlCfgKey, defCapacity);
                return false;
            }
            bucketInfo.Tokens -= 1;
            await _endPointConfigure.UpdateTokenBucket(flowControlCfgKey, bucketInfo);
            return true;
        }
    }
}
