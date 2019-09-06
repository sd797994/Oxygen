using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.IServerFlowControl
{
    public interface ITokenBucket
    {
        /// <summary>
        /// 初始化桶
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="rate"></param>
        void InitTokenBucket(long capacity, long rate);
        /// <summary>
        /// 检查令牌是否充足
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        bool Grant(string key, ServiceConfigureInfo serviceInfo);
    }
}
