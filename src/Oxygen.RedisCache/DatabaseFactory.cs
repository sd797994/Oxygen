using Oxygen.CommonTool;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Oxygen.RedisCache
{
    public class DatabaseFactory
    {
        private static ConcurrentDictionary<int,IDatabase> _database;
        private static ConnectionMultiplexer _connection;
        static DatabaseFactory()
        {
            _connection = _connection ?? ConnectionMultiplexer.Connect(OxygenSetting.RedisAddress);
            _database = _database ?? new ConcurrentDictionary<int, IDatabase>();
        }

        /// <summary>
        /// 获取或创建数据库
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IDatabase GetDatabase(int key = 0)
        {
            if (!_database.TryGetValue(key, out IDatabase databse))
            {
                databse = _connection.GetDatabase(key);
                _database.TryAdd(key, databse);
            }
            return databse;
        }
        /// <summary>
        /// 释放redis连接
        /// </summary>
        public static void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
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
