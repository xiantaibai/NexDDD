using System.Xml.Linq;
using testApi.Events;
using testApi.MultiTenantDbContext;
using Yitter.IdGenerator;
using YSCore.Base.DatabaseAccessor;
using YSCore.Base.DatabaseAccessor.Filter;

namespace testApi.Entites;

public class Blog : BaseEntity, ITenantIdFilter
{
    public string Name { get; private set; }

    public string? Url { get; private set; }

    private readonly List<Post> _posts;

    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    public long TenantId { get; set; } = 0;

    protected Blog()
    {
        _posts = new List<Post>();
    }

    public Blog(string name, string url)
    {
        Name = name;
        Url = url;
        Id = YitIdHelper.NextId();
        CreateTime = DateTime.Now;
        AddDomainEvent(new CreateBlogDomainEvent(this));
    }

    public void AddPosts(List<Post> posts)
    {
        _posts.AddRange(posts);
    }
}