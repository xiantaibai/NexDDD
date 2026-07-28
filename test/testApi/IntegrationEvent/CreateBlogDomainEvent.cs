using MediatR;
using Microsoft.Extensions.Logging;
using testApi.Entites;
using NexDDD.Core.DomainEvent;
using NexDDD.Core.Integration;

namespace testApi.Events;

/// <summary>
/// 表示博客创建完成后对外发布的稳定集成事件契约。
/// </summary>
public record CreateBlogDomainIntegrationEvent(string name,string code) : IIntegrationEvent;

/// <summary>
/// 负责消费博客创建集成事件，并演示通过日志记录消息处理结果。
/// </summary>
public class CreateBlogDomainIntegrationEventHandler(ILogger<CreateBlogDomainIntegrationEventHandler> logger) : IIntegrationEventHandler<CreateBlogDomainIntegrationEvent>
{
    public Task HandleAsync(IntegrationEventHandlerContext eventData, CancellationToken cancellationToken = default)
    {
        var data = (CreateBlogDomainIntegrationEvent)eventData.Source;
        var header = eventData.Properties;

        logger.LogInformation("接受 消息队列数据,{data}", data);

        return Task.CompletedTask;
    }
}
