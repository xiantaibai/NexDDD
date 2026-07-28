using System;
using System.Text.Json;

namespace NexDDD.Core.Context
{
    /// <summary>
    /// 上下文 序列化
    /// 定义请求上下文模块的协作契约，约束调用方与具体实现之间的依赖边界。
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


    /// <summary>
    /// 提供请求上下文模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
    /// </summary>
    public class JsonContextSerializer : IContextSerializer
    {
        public string Serialize<T>(T context) => JsonSerializer.Serialize(context);
        public T Deserialize<T>(string data) where T : class => string.IsNullOrEmpty(data) ? null : JsonSerializer.Deserialize<T>(data);
    }
}
