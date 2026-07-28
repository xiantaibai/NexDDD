using testApi.Entites;
using NexDDD.Core.DomainEvent;
using NexDDD.Core.Integration;

namespace testApi.Events;

/// <summary>
/// 承载领域事件流程中的不可变消息数据，供处理器、管线或基础设施组件传递。
/// </summary>
public record CreateBlogDomainEvent(Blog blog) : IDomainEvent;

/// <summary>
/// 负责领域事件流程的处理与编排，协调上游请求和下游服务完成当前用例。
/// </summary>
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