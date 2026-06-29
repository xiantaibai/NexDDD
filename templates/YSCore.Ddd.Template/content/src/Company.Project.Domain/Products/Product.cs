using Company.Project.Domain.Products.Events;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.Exceptions;

namespace Company.Project.Domain.Products;

/// <summary>
/// 作为商品聚合根保护名称、库存编码和价格规则，并记录商品生命周期中的领域事实。
/// </summary>
public sealed class Product : BaseEntity, IAggregateRoot
{
    private Product()
    {
    }

    /// <summary>
    /// 创建满足商品领域规则的聚合根。
    /// </summary>
    public Product(long id, string name, string sku, decimal price)
    {
        if (id <= 0)
        {
            throw ExceptionHelper.Bah("商品主键必须大于零");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw ExceptionHelper.Bah("商品库存编码不能为空");
        }

        Id = id;
        Sku = sku.Trim();
        CreateTime = DateTime.UtcNow;

        ChangeName(name);
        ChangePrice(price);
        AddDomainEvent(new ProductCreatedDomainEvent(Id, Name, Sku, Price));
    }

    /// <summary>
    /// 获取商品名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 获取商品库存编码。
    /// </summary>
    public string Sku { get; private set; } = string.Empty;

    /// <summary>
    /// 获取商品价格。
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// 按领域规则修改商品名称。
    /// </summary>
    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw ExceptionHelper.Bah("商品名称不能为空");
        }

        Name = name.Trim();
    }

    /// <summary>
    /// 按领域规则修改商品价格。
    /// </summary>
    public void ChangePrice(decimal price)
    {
        if (price < 0)
        {
            throw ExceptionHelper.Bah("商品价格不能小于零");
        }

        Price = price;
    }
}
