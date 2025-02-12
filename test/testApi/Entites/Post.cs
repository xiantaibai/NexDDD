using testApi.MultiTenantDbContext;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.DatabaseAccessor.Filter;

namespace testApi.Entites;

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