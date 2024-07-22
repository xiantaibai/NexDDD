using Yitter.IdGenerator;

namespace YS.Provider.Snowflake
{
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
