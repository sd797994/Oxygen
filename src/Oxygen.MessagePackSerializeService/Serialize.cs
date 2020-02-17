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
        public static Lazy<MessagePackSerializerOptions> loadConfig = new Lazy<MessagePackSerializerOptions>(() =>
        {
            return MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
               NativeDateTimeResolver.Instance,
               ContractlessStandardResolverAllowPrivate.Instance,
               StandardResolver.Instance));
        });
        public Serialize(IOxygenLogger logger)
        {
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
                return MessagePackSerializer.Serialize(input, loadConfig.Value);
            }
            catch (Exception e)
            {
                _logger.LogError($"序列化对象失败：{e.Message}");
            }
            return default(byte[]);
        }
        /// <summary>
        /// 序列化T为JSON字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public string SerializesJson<T>(T input)
        {
            if (input == null)
                return default(string);
            try
            {
                return MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(input, loadConfig.Value), loadConfig.Value);
            }
            catch (Exception e)
            {
                _logger.LogError($"序列化对象失败：{e.Message}");
            }
            return default(string);
        }
        /// <summary>
        /// 序列化json字符串为Byte[]
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public byte[] SerializesJsonString(string jsonStr)
        {
            if (jsonStr == null)
                return default(byte[]);
            try
            {
                return MessagePackSerializer.ConvertFromJson(jsonStr, loadConfig.Value);
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
                return MessagePackSerializer.Deserialize<T>(input, loadConfig.Value);
            }
            catch (Exception e)
            {
                _logger.LogError($"反序化对象失败：{e.Message}");
            }
            return default(T);
        }

        /// <summary>
        /// 反序列化JSON字符串为T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public T DeserializesJson<T>(string input)
        {
            if (input == null || !input.Any())
                return default(T);
            try
            {
                return MessagePackSerializer.Deserialize<T>(SerializesJsonString(input), loadConfig.Value);
            }
            catch (Exception e)
            {
                _logger.LogError($"反序化对象失败：{e.Message}，消息体：{input}");
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
                return MessagePackSerializer.Serialize(type, input, loadConfig.Value);
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
                return MessagePackSerializer.Deserialize(type, input, loadConfig.Value);
            }
            catch (Exception e)
            {
                _logger.LogError($"反序化对象失败：{e.Message}");
            }
            return null;
        }
    }
}