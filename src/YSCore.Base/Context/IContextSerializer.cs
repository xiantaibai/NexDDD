using System;
using System.Text.Json;

namespace YSCore.Base.Context
{
    /// <summary>
    /// 上下文 序列化
    /// </summary>
    public interface IContextSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        string Serialize<T>(T context);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        T Deserialize<T>(string data) where T : class;
    }


    public class JsonContextSerializer : IContextSerializer
    {
        public string Serialize<T>(T context) => JsonSerializer.Serialize(context);
        public T Deserialize<T>(string data) where T : class => string.IsNullOrEmpty(data) ? null : JsonSerializer.Deserialize<T>(data);
    }
}
