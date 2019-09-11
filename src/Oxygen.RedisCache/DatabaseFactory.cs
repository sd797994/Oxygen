using Oxygen.CommonTool;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Oxygen.RedisCache
{
    /// <summary>
    /// 缓存客户端工厂
    /// </summary>
    public class DatabaseFactory
    {
        private static Lazy<ConcurrentDictionary<int, IDatabase>> _database = new Lazy<ConcurrentDictionary<int, IDatabase>>(() => { return new ConcurrentDictionary<int, IDatabase>(); });
        private static Lazy<ConnectionMultiplexer> _connection = new Lazy<ConnectionMultiplexer>(() => { return ConnectionMultiplexer.Connect(OxygenSetting.RedisAddress); });
        /// <summary>
        /// 获取或创建数据库
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnection()
        {
            return _connection.Value;
        }
        /// <summary>
        /// 获取或创建数据库
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IDatabase GetDatabase(int key = 0)
        {
            if (!_database.Value.TryGetValue(key, out IDatabase databse))
            {
                databse = _connection.Value.GetDatabase(key);
                _database.Value.TryAdd(key, databse);
            }
            return databse;
        }
        /// <summary>
        /// 释放redis连接
        /// </summary>
        public static void Dispose()
        {
            if (_connection != null)
                _connection.Value.Dispose();
        }

        /// <summary>
        /// 创建锁工厂
        /// </summary>
        /// <returns></returns>
        public static RedLockFactory GetLockFacroty()
        {
            return new RedLockFactory(new RedLockConfiguration(new List<RedLockEndPoint>() { new RedLockEndPoint() {
                EndPoint=OxygenSetting.RedisAddressEndPoint
            } }));
        }
    }
}
