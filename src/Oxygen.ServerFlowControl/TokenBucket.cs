using Oxygen.CommonTool;
using Oxygen.ICache;
using Oxygen.IServerFlowControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.ServerFlowControl
{
    public class TokenBucket: ITokenBucket
    {
        public static ICacheService _cacheService;
        public TokenBucket(ICacheService cacheService)
        {
            _cacheService = cacheService;
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
            return _cacheService.BlockingWork($"{OxygenSetting.TokenLimitSettingKey}{key}", new TimeSpan(0, 0, 30), new TimeSpan(0, 0, 30), () =>
            {
                var bucketInfo = GetOrAddTokenBucket(key, serviceInfo);
                UpdateTokens(bucketInfo);
                if (bucketInfo.Tokens < 1)
                {
                    var timeToIntervalEnd = bucketInfo.StartTimeStamp - DateTime.UtcNow.Ticks;
                    if (timeToIntervalEnd < 0) return Grant(key, serviceInfo);
                    return false;
                }
                bucketInfo.Tokens -= 1;
                UpdateTokenBucket(key, bucketInfo);
                return true;
            });
        }
        #region 私有方法
        /// <summary>
        /// 更新令牌
        /// </summary>
        /// <param name="bucketInfo"></param>
        protected void UpdateTokens(TokenBucketInfo bucketInfo)
        {
            var currentTime = DateTime.UtcNow.Ticks;
            if (currentTime < bucketInfo.StartTimeStamp)
                return;
            bucketInfo.Tokens = Capacity;
            bucketInfo.StartTimeStamp = currentTime + Rate;
        }
        /// <summary>
        /// 获取或创建令牌桶info
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private TokenBucketInfo GetOrAddTokenBucket(string key, ServiceConfigureInfo serviceInfo)
        {
            var bucketInfo = _cacheService.GetHashCache<TokenBucketInfo>(OxygenSetting.TokenLimitSettingKey, key);
            if (bucketInfo == null)
            {
                bucketInfo = new TokenBucketInfo();
                bucketInfo.Tokens = serviceInfo.DefCapacity;
                bucketInfo.StartTimeStamp = DateTime.UtcNow.Ticks;
                _cacheService.SetHashCache(OxygenSetting.TokenLimitSettingKey, key, bucketInfo);
            }
            return bucketInfo;
        }

        /// <summary>
        /// 更新令牌桶info
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="bucketInfo"></param>
        private void UpdateTokenBucket(string key, TokenBucketInfo bucketInfo)
        {
            _cacheService.SetHashCache(OxygenSetting.TokenLimitSettingKey, key, bucketInfo);
        }
        #endregion
    }
}
