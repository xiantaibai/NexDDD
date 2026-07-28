using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Provider.Snowflake
{
    /// <summary>
    /// 定义雪花 ID 模块的协作契约，约束调用方与具体实现之间的依赖边界。
    /// </summary>
    public interface ISnowflakeIdGenerator
    {
        /// <summary>
        /// 获取下一个ID
        /// </summary>
        /// <returns></returns>
        long NextId();
    }
}
