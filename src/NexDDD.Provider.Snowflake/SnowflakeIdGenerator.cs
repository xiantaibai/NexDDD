using Yitter.IdGenerator;

namespace NexDDD.Provider.Snowflake
{
    /// <summary>
    /// 负责雪花 ID 模块中的 SnowflakeIdGenerator 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class SnowflakeIdGenerator : ISnowflakeIdGenerator
    {
        public long NextId()
        {
            return YitIdHelper.NextId();
        }

        public static long GetNextValue()
        {
            return YitIdHelper.NextId();
        }
    }
}
