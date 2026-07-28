namespace Company.Project.Application.Contracts.Products;

/// <summary>
/// 集中维护商品用例向外暴露的稳定错误码。
/// 负责NexDDD 核心模块中的 ProductErrorCodes 能力，并限定其与调用方及基础设施组件的协作边界。
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
