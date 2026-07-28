using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexDDD.Provider.Snowflake;

namespace NexDDD.Provider.EntityFrameworkCore.ValueGenerators
{
    /// <summary>
    /// 负责EF Core 持久化模块中的 IdGenerator 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class IdGenerator : ValueGenerator<long>
    {
        public override bool GeneratesTemporaryValues => false;

        public override long Next(EntityEntry entry)
        {
            return SnowflakeIdGenerator.GetNextValue();
        }
    }
}
