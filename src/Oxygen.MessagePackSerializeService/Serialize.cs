using MessagePack;
using MessagePack.Resolvers;
using Oxygen.CommonTool.Logger;
using Oxygen.ISerializeService;
using System;
using System.Linq;

namespace Oxygen.MessagePackSerializeService
{
    /// <summary>
    /// 序列化服务
    /// </summary>
    public class Serialize : ISerialize
    {
        private readonly IOxygenLogger _logger;
        public static Lazy<bool> loadConfig = new Lazy<bool>(()=> {
            CompositeResolver.RegisterAndSetAsDefault(
               NativeDateTimeResolver.Instance,
               ContractlessStandardResolverAllowPrivate.Instance);
            return true;
        });
        public Serialize(IOxygenLogger logger)
        {
            _ = loadConfig.Value;
            _logger = logger;
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Serializes<T>(T input)
        {
            if (input == null)
                return default(byte[]);
            try
            {
                return MessagePackSerializer.Serialize(input);
            }
            catch (Exception e)
            {
                _logger.LogError($"序列化对象失败：{e.Message}");
            }
            return default(byte[]);
        }
        /// <summary>
        /// 序列化json字符串
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public byte[] SerializesJsonString(string jsonStr)
        {
            if (jsonStr == null)
                return default(byte[]);
            try
            {
                return MessagePackSerializer.FromJson(jsonStr);
            }
            catch (Exception e)
            {
                _logger.LogError($"序列化对象失败：{e.Message}");
            }
            return default(byte[]);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public T Deserializes<T>(byte[] input)
        {
            if (input == null || !input.Any())
                return default(T);
            try
            {
                return MessagePackSerializer.Deserialize<T>(input);
            }
            catch (Exception e)
            {
                _logger.LogError($"反序化对象失败：{e.Message}");
            }
            return default(T);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Serializes(Type type, object input)
        {
            if (input == null)
                return default(byte[]);
            try
            {
                return MessagePackSerializer.NonGeneric.Serialize(type, input);
            }
            catch (Exception e)
            {
                _logger.LogError($"序列化对象失败：{e.Message}");
            }
            return default(byte[]);
        }

        public object Deserializes(Type type, byte[] input)
        {
            if (input == null || !input.Any())
                return null;
            try
            {
                return MessagePackSerializer.NonGeneric.Deserialize(type, input);
            }
            catch (Exception e)
            {
                _logger.LogError($"反序化对象失败：{e.Message}");
            }
            return null;
        }
    }
}