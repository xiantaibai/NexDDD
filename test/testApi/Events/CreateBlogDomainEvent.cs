using testApi.Entites;
using YSCore.Base.DomainEvent;

namespace testApi.Events;

public record CreateBlogDomainEvent(Blog blog) :IDomainEvent;

public class CreateBlogDomainEventHandler : IDomainEventHandler<CreateBlogDomainEvent>
{
    public Task Handle(CreateBlogDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"创建Blog 成功,{notification.blog.Name}");

        return Task.CompletedTask;
    }
}