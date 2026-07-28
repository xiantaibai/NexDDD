using testApi.MultiTenantDbContext;
using NexDDD.Core.DatabaseAccessor;
using NexDDD.Core.DatabaseAccessor.Filter;

namespace testApi.Entites;

/// <summary>
/// 用于验证示例与回归验证相关行为，确保框架重命名后契约和运行结果保持正确。
/// </summary>
public class Post : BaseEntity, ITenantIdFilter
{
    public long TenantId { get; set; }
    public string? Title { get; private set; }
    public string? Content { get; private set; }

    public long? BlogId { get; private set; }
    public Blog? Blog { get; }

    protected Post()
    {
    }

    public Post(long blogId, string title, string content)
    {
        BlogId = blogId;
        Title = title;
        Content = content;
    }
}