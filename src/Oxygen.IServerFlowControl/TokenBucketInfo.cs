using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.IServerFlowControl
{
    /// <summary>
    /// 全局令牌桶
    /// </summary>
    public class TokenBucketInfo
    {
        public long StartTimeStamp { get; set; }
        public long Tokens { get; set; }
    }
}
