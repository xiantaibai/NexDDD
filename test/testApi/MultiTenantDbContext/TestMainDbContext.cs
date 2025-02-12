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
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace testApi.MultiTenantDbContext;

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