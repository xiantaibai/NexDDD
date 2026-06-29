namespace Company.Project.Application.Contracts.Products;

/// <summary>
/// 承载商品查询结果，避免向接口调用方暴露领域实体和持久化细节。
/// </summary>
public sealed class ProductDto
{
    /// <summary>
    /// 获取或设置商品主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 获取或设置商品名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置商品库存编码。
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置商品价格。
    /// </summary>
    public decimal Price { get; set; }
}
