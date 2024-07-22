using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using YS.Provider.Snowflake;

namespace YS.Provider.EntityFrameworkCore.ValueGenerators
{
    public class IdGenerator : ValueGenerator<long>
    {
        public override bool GeneratesTemporaryValues => false;

        public override long Next(EntityEntry entry)
        {
            return SnowflakeIdGenerator.GetNextValue();
        }
    }
}
