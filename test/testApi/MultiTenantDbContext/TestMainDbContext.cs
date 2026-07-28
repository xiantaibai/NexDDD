using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NexDDD.Provider.EntityFrameworkCore;
using NexDDD.Core.DatabaseAccessor.Filter;
using NexDDD.Core.MultiTenants;
using NexDDD.Provider.EntityFrameworkCore.Extensions;
using testApi.Entites;
using testApi.EntityConfigurations;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace testApi.MultiTenantDbContext;

/// <summary>
/// 提供请求上下文模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
/// </summary>
public class TestMainDbContext : AbstractShardingDbContext, IShardingTableDbContext
{
    public IServiceProvider ServiceProvider { get; }
    public TestMainDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options)
    {
        ServiceProvider= serviceProvider;
    }

    public DbSet<Blog>? Blogs { get; set; }
    public DbSet<Post>? Posts { get; set; }

    public IRouteTail RouteTail { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        base.OnModelCreating(modelBuilder);

        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(Blog).Assembly);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogTypeConfiguration).Assembly);

        modelBuilder.AddQueryFilter<IDeletedFilter>(x => x.IsDelete == false);
    }
}