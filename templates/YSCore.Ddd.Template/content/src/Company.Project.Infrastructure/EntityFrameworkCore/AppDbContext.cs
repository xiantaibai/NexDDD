using Company.Project.Domain.Products;
using Microsoft.EntityFrameworkCore;
using YSCore.Provider.EntityFrameworkCore;

namespace Company.Project.Infrastructure.EntityFrameworkCore;

/// <summary>
/// 承载应用写模型与默认读模型，并通过 YSCore 工作单元协调事务和领域事件分发。
/// </summary>
public sealed class AppDbContext : AppDbContextBase
{
    /// <summary>
    /// 创建应用数据库上下文。
    /// </summary>
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    /// <summary>
    /// 获取商品实体集合。
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
