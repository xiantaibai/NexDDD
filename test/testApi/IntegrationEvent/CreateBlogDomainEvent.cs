using Microsoft.Extensions.Logging;
using testApi.Entites;
using YSCore.Base.DomainEvent;
using YSCore.Base.Integration;

namespace testApi.Events;

public record CreateBlogDomainIntegrationEvent(string name,string code) : IIntegrationEvent;

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