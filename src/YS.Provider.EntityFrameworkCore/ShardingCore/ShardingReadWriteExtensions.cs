using System.ComponentModel;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using YSCore.Provider.EntityFrameworkCore;
using ShardingCore.Sharding;

namespace YSCore.Provider.EntityFrameworkCore.ShardingCore
{
    public static class ShardingReadWriteExtensions
    {
        public static T UseReadOnly<T>(this T context)
        where T : AbstractShardingDbContext
        {
            context.ReadWriteSeparationReadOnly();
            return context;
        }

        public static T UseWriteOnly<T>(this T context)
            where T : AbstractShardingDbContext
        {
            context.ReadWriteSeparationWriteOnly();
            return context;
        }
    }
}