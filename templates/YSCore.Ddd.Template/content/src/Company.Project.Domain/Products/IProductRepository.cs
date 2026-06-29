using YSCore.Base.DatabaseAccessor.Repositories;

namespace Company.Project.Domain.Products;

/// <summary>
/// 定义商品聚合写侧所需的持久化边界，由基础设施层提供具体实现。
/// </summary>
public interface IProductRepository : IYsRepository<Product, long>
{
    /// <summary>
    /// 从写库按库存编码加载商品，用于创建等写用例中的唯一性判断。
    /// </summary>
    Task<Product?> FindBySkuAsync(string sku, CancellationToken cancellationToken = default);
}
