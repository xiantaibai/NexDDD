using YSCore.Base.Events;

namespace Company.Project.Application.Contracts.Products;

/// <summary>
/// 表达创建商品的写入意图，并作为创建商品用例的稳定输入契约。
/// </summary>
public sealed record CreateProductCommand(string Name, string Sku, decimal Price) : ICommand<long>;
