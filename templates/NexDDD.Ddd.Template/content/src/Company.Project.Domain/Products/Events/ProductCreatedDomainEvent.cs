using NexDDD.Core.DomainEvent;

namespace Company.Project.Domain.Products.Events;

/// <summary>
/// 表示商品聚合已经成功创建的服务内部领域事实。
/// </summary>
public sealed record ProductCreatedDomainEvent(long ProductId, string Name, string Sku, decimal Price)
    : IDomainEvent;
