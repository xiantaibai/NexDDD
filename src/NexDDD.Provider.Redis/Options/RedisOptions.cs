using StackExchange.Redis.Extensions.Core.Configuration;

namespace NexDDD.Provider.Redis
{
    /// <summary>
    /// 定义Redis 客户端实例及其序列化策略，由依赖注入注册过程读取并创建连接。
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// 获取或设置 Redis 连接集合；每个元素描述一个可注册的 Redis 实例。
        /// </summary>
        public IEnumerable<RedisConfiguration> Instances { get; set; }

        /// <summary>
        /// 获取或设置对象序列化方式，默认使用 System.Text.Json。
        /// </summary>
        public SerializeTypeEnum SerializeType { get; set; } = SerializeTypeEnum.TextJson;

    }

    /// <summary>
    /// 定义Redis 值支持的对象序列化格式，供客户端读写过程选择编码器。
    /// </summary>
    public enum SerializeTypeEnum
    {
        TextJson = 1,
        Protobuf = 3
    }
}
