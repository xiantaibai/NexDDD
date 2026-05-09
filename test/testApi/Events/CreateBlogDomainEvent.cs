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

        IDictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("userId", "123456");
        headers.Add("userName", "测试");

        if (notification.blog.Name == "异常")
        {
            throw new Exception("测试异常");
        }
        // await publisher.PublishAsync(new CreateBlogDomainIntegrationEvent(notification.blog.Name, notification.blog.Url),headers);

        //return Task.CompletedTask;
    }
}