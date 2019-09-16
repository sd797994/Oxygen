using CSRedis;
using Oxygen.CommonTool;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.RedisCache
{
    /// <summary>
    /// 缓存客户端工厂
    /// </summary>
    public class CsRedisFactory
    {
        static Lazy<CSRedisClient> client = new Lazy<CSRedisClient>(() => { return new CSRedisClient(OxygenSetting.RedisAddress); });
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static CSRedisClient GetDatabase()
        {
            return client.Value;
        }
    }
}
