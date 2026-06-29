using Company.Project.Domain.Products;
using Company.Project.Domain.Products.Events;
using Xunit;

namespace Company.Project.Domain.Tests.Products;

/// <summary>
/// 验证商品聚合对核心业务规则和领域事件的保护行为。
/// </summary>
public sealed class ProductTests
{
    /// <summary>
    /// 验证创建合法商品时会记录商品创建领域事件。
    /// </summary>
    [Fact]
    public void CreateProduct_ShouldRaiseProductCreatedDomainEvent()
    {
        var product = new Product(1, "测试商品", "SKU-001", 99.90m);

        var domainEvent = Assert.Single(product.GetDomainEvents());
        var createdEvent = Assert.IsType<ProductCreatedDomainEvent>(domainEvent);

        Assert.Equal(product.Id, createdEvent.ProductId);
        Assert.Equal(product.Sku, createdEvent.Sku);
    }

    /// <summary>
    /// 验证负数价格会被商品聚合拒绝。
    /// </summary>
    [Fact]
    public void ChangePrice_WithNegativePrice_ShouldThrow()
    {
        var product = new Product(1, "测试商品", "SKU-001", 99.90m);

        Assert.ThrowsAny<Exception>(() => product.ChangePrice(-1));
    }
}
