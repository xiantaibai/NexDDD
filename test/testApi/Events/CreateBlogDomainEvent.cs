using testApi.Entites;
using YSCore.Base.DomainEvent;
using YSCore.Base.Integration;

namespace testApi.Events;

public record CreateBlogDomainEvent(Blog blog) : IDomainEvent;

public class CreateBlogDomainEventHandler(IIntegrationEventPublisher publisher) : IDomainEventHandler<CreateBlogDomainEvent>
{
    public async Task Handle(CreateBlogDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"创建Blog 成功,{notification.blog.Name}");

        await publisher.PublishAsync(new CreateBlogDomainIntegrationEvent(notification.blog.Name, notification.blog.Url));

        //return Task.CompletedTask;
    }
}