using System;

namespace Oxygen.ISerializeService
{
    /// <summary>
    /// 序列化接口
    /// </summary>
    public interface ISerialize
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        byte[] Serializes<T>(T input);
        /// <summary>
        /// 序列化json字符串
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        byte[] SerializesJsonString(string jsonStr);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        T Deserializes<T>(byte[] input);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        object Deserializes(Type type, byte[] input);
    }
}
