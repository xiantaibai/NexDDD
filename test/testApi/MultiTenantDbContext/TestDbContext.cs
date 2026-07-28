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

namespace testApi.MultiTenantDbContext;

/// <summary>
/// 提供请求上下文模块的运行上下文，协调状态生命周期及其与依赖组件的交互。
/// </summary>
public class TestDbContext : ShardingTableDbContext
{
    private readonly ITenantResolver _tenantResolver;

    public TestDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
        // _tenantResolver = GetService<ITenantResolver>();
    }

    public DbSet<Blog>? Blogs { get; set; }
    public DbSet<Post>? Posts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(Blog).Assembly);
        // 租户 过滤器
        // var TenantId = _tenantResolver.GetTenantId();
       
       //  modelBuilder.AddQueryFilter<ITenantIdFilter>(x => x.TenantId == TenantId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogTypeConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}