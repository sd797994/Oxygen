using Oxygen.ICache;
using Oxygen.ISerializeService;
using System;
using System.Threading.Tasks;

namespace Oxygen.RedisCache
{
    /// <summary>
    /// redis缓存服务
    /// </summary>
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
            return CsRedisFactory.GetDatabase().Exists(key);
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

            var value = CsRedisFactory.GetDatabase().Get<byte[]>(key);
            if (value == null)
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

            CsRedisFactory.GetDatabase().Set(key, _serialize.Serializes<T>(value));
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

            int.TryParse(expiressAbsoulte.TotalSeconds.ToString(), out int expiressSecond);
            CsRedisFactory.GetDatabase().Set(key, _serialize.Serializes<T>(value), expiressSecond);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            CsRedisFactory.GetDatabase().Del(key);
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


            var value = CsRedisFactory.GetDatabase().HGet<byte[]>(key, filed);
            if (value == null)
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

            if (CsRedisFactory.GetDatabase().HExists(key, filed))
                CsRedisFactory.GetDatabase().HDel(key, filed);
            CsRedisFactory.GetDatabase().HSet(key, filed, _serialize.Serializes<T>(value));
        }
        /// <summary>
        /// 发布消息到对应主题
        /// </summary>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <param name="value"></param>
        public async Task PublishAsync<T>(string channel, T value)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentNullException(nameof(channel));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            await CsRedisFactory.GetDatabase().PublishAsync(channel, _serialize.SerializesJson(value));
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public void Subscribe<T>(string channel, Action<T> func)
        {
            CsRedisFactory.GetDatabase().Subscribe((channel, message =>
            {
                func.Invoke(_serialize.DeserializesJson<T>(message.Body));
            }
            ));
        }
    }
}
