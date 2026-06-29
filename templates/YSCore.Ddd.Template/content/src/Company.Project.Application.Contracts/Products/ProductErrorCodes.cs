namespace Company.Project.Application.Contracts.Products;

/// <summary>
/// 集中维护商品用例向外暴露的稳定错误码。
/// </summary>
public static class ProductErrorCodes
{
    /// <summary>
    /// 商品不存在。
    /// </summary>
    public const string NotFound = "Product.NotFound";

    /// <summary>
    /// 商品库存编码已存在。
    /// </summary>
    public const string SkuExists = "Product.SkuExists";
}
