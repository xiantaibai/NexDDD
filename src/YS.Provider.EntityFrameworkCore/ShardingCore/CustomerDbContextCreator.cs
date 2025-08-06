using Microsoft.EntityFrameworkCore;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Sharding.Abstractions;

namespace YSCore.Provider.EntityFrameworkCore.ShardingCore;
{
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
