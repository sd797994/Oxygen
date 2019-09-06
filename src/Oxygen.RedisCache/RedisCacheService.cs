using Oxygen.ICache;
using Oxygen.ISerializeService;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.RedisCache
{
    public class RedisCacheService : ICacheService
    {
        private readonly ISerialize _serialize;
        public RedisCacheService(ISerialize serialize)
        {
            _serialize = serialize;
        }
        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            return DatabaseFactory.GetDatabase().KeyExists(key);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetCache<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));


            var value = DatabaseFactory.GetDatabase().StringGet(key);
            if (!value.HasValue)
                return default(T);


            return _serialize.Deserializes<T>(value);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetCache<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            if (Exists(key))
                RemoveCache(key);


            DatabaseFactory.GetDatabase().StringSet(key, _serialize.Serializes<T>(value));
        }
        /// <summary>
        /// 设置缓存带过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiressAbsoulte"></param>
        public void SetCache<T>(string key, T value, TimeSpan expiressAbsoulte)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            if (Exists(key))
                RemoveCache(key);


            DatabaseFactory.GetDatabase().StringSet(key, _serialize.Serializes<T>(value), expiressAbsoulte);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));


            DatabaseFactory.GetDatabase().KeyDelete(key);
        }

        /// <summary>
        /// 强制回收redis连接
        /// </summary>
        public void Dispose()
        {
            DatabaseFactory.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 获取哈希缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <returns></returns>
        public T GetHashCache<T>(string key, string filed) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filed))
                throw new ArgumentNullException(nameof(filed));


            var value = DatabaseFactory.GetDatabase().HashGet(key, filed);
            if (!value.HasValue)
                return default(T);
            return _serialize.Deserializes<T>(value);
        }

        /// <summary>
        /// 设置哈希缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetHashCache<T>(string key, string filed, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filed))
                throw new ArgumentNullException(nameof(filed));
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            if (DatabaseFactory.GetDatabase().HashExists(key, filed))
                DatabaseFactory.GetDatabase().HashDelete(key, filed);
            DatabaseFactory.GetDatabase().HashSet(key, filed, _serialize.Serializes<T>(value));
        }
        /// <summary>
        /// 获取分布式锁
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="expiryTime"></param>
        /// <param name="waitTime"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        public bool BlockingWork(string resource, TimeSpan expiryTime, TimeSpan waitTime, Func<bool> work)
        {
            using (var redisLockFactory = DatabaseFactory.GetLockFacroty())
            {
                using (var redisLock = redisLockFactory.CreateLock(resource, expiryTime, waitTime, TimeSpan.FromSeconds(1)))
                {
                    if (redisLock.IsAcquired)
                    {
                        if (work())
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
