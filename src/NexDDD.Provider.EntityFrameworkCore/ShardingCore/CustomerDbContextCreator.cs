using Microsoft.EntityFrameworkCore;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Sharding.Abstractions;

namespace NexDDD.Provider.EntityFrameworkCore.ShardingCore
{
    /// <summary>
    /// 提供EF Core 持久化模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
    /// </summary>
    public class CustomerDbContextCreator : IDbContextCreator
    {
        public DbContext CreateDbContext(DbContext mainDbContext, ShardingDbContextOptions shardingDbContextOptions)
        {
            var dbContext = new ShardingTableDbContext((DbContextOptions<ShardingTableDbContext>)shardingDbContextOptions.DbContextOptions, ((ShardingTableDbContext)mainDbContext)._serviceProvider);
            Console.WriteLine("IsFrozen" + shardingDbContextOptions.DbContextOptions.IsFrozen);
            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
            }
            _ = dbContext.Model;
            return dbContext;
        }

        public DbContext GetShellDbContext(IShardingProvider shardingProvider)
        {
            return shardingProvider.GetService<ShardingTableDbContext>();
        }
    }
}
