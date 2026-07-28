using Microsoft.EntityFrameworkCore;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Sharding.Abstractions;

namespace testApi.MultiTenantDbContext
{
    /// <summary>
    /// 提供请求上下文模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
    /// </summary>
    public class CustomerDbContextCreator : IDbContextCreator
    {
        public DbContext CreateDbContext(DbContext mainDbContext, ShardingDbContextOptions shardingDbContextOptions)
        {
            var dbContext = new TestDbContext((DbContextOptions<TestDbContext>)shardingDbContextOptions.DbContextOptions, ((TestDbContext)mainDbContext)._serviceProvider);
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
            return shardingProvider.GetService<TestDbContext>();
        }
    }
}
