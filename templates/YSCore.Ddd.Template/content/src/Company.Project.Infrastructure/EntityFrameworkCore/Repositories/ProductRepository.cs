using Company.Project.Domain.Products;
using Microsoft.EntityFrameworkCore;
using YSCore.Provider.EntityFrameworkCore;

namespace Company.Project.Infrastructure.EntityFrameworkCore.Repositories;

/// <summary>
/// 使用 EF Core 实现商品聚合写仓储，负责从主库加载和持久化商品聚合。
/// </summary>
public sealed class ProductRepository
    : YsRepositoryBase<Product, long, AppDbContext>, IProductRepository
{
    /// <summary>
    /// 创建商品仓储。
    /// </summary>
    public ProductRepository(AppDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public Task<Product?> FindBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return DbContext.Set<Product>()
            .FirstOrDefaultAsync(x => x.Sku == sku, cancellationToken);
    }
}
