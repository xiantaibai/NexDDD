using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using YSCore.Provider.EntityFrameworkCore;
using YSCore.Base.DatabaseAccessor.Filter;
using YSCore.Base.MultiTenants;
using YSCore.Provider.EntityFrameworkCore.Extensions;
using testApi.Entites;
using testApi.EntityConfigurations;
using ShardingCore.Core.DbContextCreator;

namespace testApi.MultiTenantDbContext;

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