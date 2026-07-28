using System.ComponentModel;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using NexDDD.Provider.EntityFrameworkCore;
using ShardingCore.Sharding;

namespace NexDDD.Provider.EntityFrameworkCore.ShardingCore
{
    /// <summary>
    /// 提供EF Core 持久化模块的注册与扩展入口，集中封装基础设施接入和通用辅助操作。
    /// </summary>
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