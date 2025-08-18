using StackExchange.Redis.Extensions.Core.Configuration;

namespace YSCore.Provider.Redis
{
    /// <summary>
    /// redis 配置项
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// redis 连接 配置
        /// </summary>
        public IEnumerable<RedisConfiguration> Instances { get; set; }

        /// <summary>
        /// 序列化方式
        /// </summary>
        public SerializeTypeEnum SerializeType { get; set; } = SerializeTypeEnum.TextJson;

    }

    /// <summary>
    /// 序列化 类型
    /// </summary>
    public enum SerializeTypeEnum
    {
        TextJson = 1,
        Protobuf = 3
    }
}
