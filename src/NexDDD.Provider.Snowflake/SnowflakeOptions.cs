using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace NexDDD.Provider.Snowflake
{
    /// <summary>
    /// 承载雪花 ID 模块的运行配置，供依赖注入注册过程与运行时组件读取。
    /// </summary>
    public class SnowflakeOptions: IdGeneratorOptions
    {
        /// <summary>
        /// 缓存工作节点前缀
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public string WorkerPrefix { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    }
}
