using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.ICache
{
    /// <summary>
    /// 通用缓存接口
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);

        /// <summary>
        /// 获取T缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetCache<T>(string key) where T : class;

        /// <summary>
        /// 设置T缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetCache<T>(string key, T value);

        /// <summary>
        /// 设置缓存带过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiressAbsoulte"></param>
        void SetCache<T>(string key, T value, TimeSpan expiressAbsoulte);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        void RemoveCache(string key);

        /// <summary>
        /// 强制回收redis连接
        /// </summary>
        void Dispose();

        /// <summary>
        /// 获取hash缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <returns></returns>
        T GetHashCache<T>(string key, string filed) where T : class;

        /// <summary>
        /// 设置哈希缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <param name="value"></param>
        void SetHashCache<T>(string key, string filed, T value);

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="expiryTime"></param>
        /// <param name="waitTime"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        bool BlockingWork(string resource, TimeSpan expiryTime, TimeSpan waitTime, Func<bool> work);
    }
}
