using Company.Project.Domain.Products.Events;
using Microsoft.Extensions.Logging;
using NexDDD.Core.DomainEvent;

namespace Company.Project.Application.Products.Events;

/// <summary>
/// 处理商品创建领域事件，并承接后续日志、通知或集成事件发布等应用侧副作用。
/// </summary>
public sealed class ProductCreatedDomainEventHandler : IDomainEventHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;

    /// <summary>
    /// 创建商品领域事件处理器。
    /// </summary>
    public ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "商品已创建，商品主键：{ProductId}，库存编码：{Sku}",
            notification.ProductId,
            notification.Sku);

        return Task.CompletedTask;
    }
}
