# NexDDD DDD 实现指南

这份参考文档给 AI 提供更具体的代码落地方式。中文说明架构思想，英文保留技术关键词，代码保持 C# 英文命名。

## 常用类型

常见 namespace：

```csharp
using NexDDD.Core.DatabaseAccessor;
using NexDDD.Core.DatabaseAccessor.Filter;
using NexDDD.Core.DatabaseAccessor.Repositories;
using NexDDD.Core.DomainEvent;
using NexDDD.Core.Events;
using NexDDD.Core.Exceptions;
using NexDDD.Core.Integration;
using NexDDD.Core.Idempotency;
using NexDDD.Core.CommandLocks;
using NexDDD.Provider.EntityFrameworkCore;
using NexDDD.Provider.EntityFrameworkCore.Extensions;
```

核心类型理解：

- `Entity<TKey>`：实体基类，包含 `Id`、相等性判断和 Domain Event collection。
- `BaseEntity`：`Entity<long>`，包含审计字段和 `IsDelete`。
- `IAggregateRoot`：Aggregate Root 标记，写 Repository 只应面向 Aggregate Root。
- `INexRepository<TEntity>` / `INexRepository<TEntity, TKey>`：聚合写仓储接口，只用于写入聚合。
- `IQueryRepository<TEntity>`：通用查询仓储接口，可面向任意 `Entity`，默认实现由 DI 注册。
- `IReadableRepository<TEntity>`：只读仓储基础接口，通常由 `IQueryRepository<TEntity>` 继承或兼容使用。
- `NexRepositoryBase<TEntity, TKey, TDbContext>`：EF Core Repository 基类。
- `ReadableRepository<TEntity, TDbContext>`：EF Core 默认查询仓储实现，可绑定只读库或从库 `DbContext`。
- `AppDbContextBase`：实现 transaction UnitOfWork 和 Domain Event dispatch 的 DbContext 基类。
- `ICommand<TResult>` / `ICommandHandler<TCommand, TResult>`：写用例模型。
- `IDomainEvent` / `IDomainEventHandler<TEvent>`：服务内部领域事件。
- `IIntegrationEvent` / `IIntegrationEventHandler<TEvent>`：跨服务集成事件。

## 标准写功能模板

### Contracts

`Command` 只表达业务意图，不包含业务规则，不创建 Entity。

```csharp
public record CreateBlogCommand(string Name, string Code) : ICommand<long>;
```

### Domain

Aggregate Root 负责保护业务不变量。外部不要直接 set 关键属性，应通过方法表达业务行为。

```csharp
public class Blog : BaseEntity, IAggregateRoot, ITenantIdFilter
{
    private readonly List<Post> _posts = new();

    protected Blog()
    {
    }

    public Blog(long id, string name, string code, long tenantId)
    {
        Id = id;
        TenantId = tenantId;
        ChangeName(name);
        Code = code;
        CreateTime = DateTime.UtcNow;

        AddDomainEvent(new BlogCreatedDomainEvent(Id, Name, Code));
    }

    public string Name { get; private set; } = default!;

    public string Code { get; private set; } = default!;

    public long TenantId { get; set; }

    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw ExceptionHelper.Bah("Blog name cannot be empty");
        }

        Name = name;
    }
}
```

### Domain Event

Domain Event 是当前服务内部事实，不是跨服务契约。

```csharp
public record BlogCreatedDomainEvent(long BlogId, string Name, string Code) : IDomainEvent;
```

### Repository Interface

写 Repository 面向 Aggregate Root，不面向任意表。写仓储用于聚合创建、修改、删除和领域写入，不作为通用读模型入口。

```csharp
public interface IBlogRepository : INexRepository<Blog, long>
{
    Task<Blog?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
```

### Application Handler

`CommandHandler` 位于 `Application`，负责业务流程编排：加载聚合、调用领域行为、协调 Repository、返回用例结果。它不手动开启 transaction，不手动 `SaveChangesAsync`。

```csharp
public class CreateBlogCommandHandler : ICommandHandler<CreateBlogCommand, long>
{
    private readonly IBlogRepository _blogRepository;
    private readonly ISnowflakeIdGenerator _idGenerator;

    public CreateBlogCommandHandler(
        IBlogRepository blogRepository,
        ISnowflakeIdGenerator idGenerator)
    {
        _blogRepository = blogRepository;
        _idGenerator = idGenerator;
    }

    public async Task<long> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var exists = await _blogRepository.FindByCodeAsync(request.Code, cancellationToken);
        if (exists != null)
        {
            throw ExceptionHelper.AppFriendly("Blog.CodeExists", request.Code);
        }

        var blog = new Blog(_idGenerator.NextId(), request.Name, request.Code, tenantId: 0);
        await _blogRepository.AddAsync(blog, cancellationToken);

        return blog.Id;
    }
}
```

### Infrastructure Repository

Repository implementation 放在 `Infrastructure`，继承 `NexRepositoryBase`。`Infrastructure` 的职责是把 Repository、DbContext、EF mapping 等数据持久化细节落地，不负责编排业务流程。

```csharp
public class BlogRepository : NexRepositoryBase<Blog, long, AppDbContext>, IBlogRepository
{
    public BlogRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<Blog?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<Blog>()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }
}
```

### EF Core Mapping

EF mapping 属于 `Infrastructure`，负责数据库结构约束和持久化配置，不承载业务规则。业务规则仍应在 Domain 中保护。

```csharp
public class BlogTypeConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.IsDelete).HasDefaultValue(false).IsRequired();
    }
}
```

### WebApi Endpoint

`WebApi` 只接收请求并转发给 `IMediator`。

```csharp
app.MapPost("/api/blogs", async (
    IMediator mediator,
    CreateBlogCommand command,
    CancellationToken cancellationToken) =>
{
    return await mediator.Send(command, cancellationToken);
});
```

## 标准读功能模板

读模型优先返回 DTO。普通读功能优先使用默认 `IQueryRepository<TEntity>`；当默认实现无法满足性能、SQL、缓存或跨数据源组合时，再定义自定义 Query Repository。

### Contracts

```csharp
public record GetBlogQuery(long Id) : IRequest<BlogDto>;

public class BlogDto
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public string Code { get; set; } = default!;
}
```

### Application QueryHandler

```csharp
public class GetBlogQueryHandler : IRequestHandler<GetBlogQuery, BlogDto>
{
    private readonly IQueryRepository<Blog> _blogQueries;

    public GetBlogQueryHandler(IQueryRepository<Blog> blogQueries)
    {
        _blogQueries = blogQueries;
    }

    public async Task<BlogDto> Handle(GetBlogQuery request, CancellationToken cancellationToken)
    {
        var dto = await _blogQueries.AsQueryable(tracking: false)
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BlogDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw ExceptionHelper.AppFriendly("Blog.NotFound");
        }

        return dto;
    }
}
```

### Custom Query Repository

当查询需要 raw SQL、Redis、本地存储、读库路由或多个数据源拼装时，定义自定义 Query Repository。它属于读模型落地，不承担业务流程编排。

```csharp
public interface IBlogQueryRepository : IQueryRepository<Blog>
{
    Task<BlogDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default);
}
```

实现放在 `Infrastructure`，可以组合 EF Core、SQL、Redis 或其他 provider：

```csharp
public class BlogQueryRepository : ReadableRepository<Blog, ReadDbContext>, IBlogQueryRepository
{
    private readonly IRedisClient _redisClient;

    public BlogQueryRepository(ReadDbContext dbContext, IRedisClient redisClient)
        : base(dbContext)
    {
        _redisClient = redisClient;
    }

    public async Task<BlogDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"blog:detail:{id}";
        var cached = await _redisClient.GetAsync<BlogDetailDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var dto = await AsQueryable(tracking: false)
            .Where(x => x.Id == id)
            .Select(x => new BlogDetailDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto != null)
        {
            await _redisClient.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return dto;
    }
}
```

如果只是简单 `Where`、`OrderBy`、`Select DTO`，不要新增 `IBlogQueryRepository`，直接使用默认 `IQueryRepository<Blog>`。

## Domain Event 转 Integration Event

当领域事件需要通知其他服务时，不要直接暴露 Domain Event。应该在 `Application` 的 Domain Event Handler 中发布 Integration Event。

```csharp
public record BlogCreatedIntegrationEvent(long BlogId, string Name, string Code) : IIntegrationEvent;

public class BlogCreatedDomainEventHandler : IDomainEventHandler<BlogCreatedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public BlogCreatedDomainEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Handle(BlogCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return _publisher.PublishAsync(
            new BlogCreatedIntegrationEvent(
                notification.BlogId,
                notification.Name,
                notification.Code),
            cancellationToken);
    }
}
```

`IntegrationEvent` payload 要稳定、明确、可版本化。不要传 `Blog`、`Order` 这类聚合对象。

## 幂等模板

当请求可能重复提交时，为 Command 增加 `IIdempotentCommand<TCommand>`。

```csharp
public class CreateBlogCommandIdempotent : IIdempotentCommand<CreateBlogCommand>
{
    public Task<IdempotencySettings> GetIdempotencyAsync(
        CreateBlogCommand command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new IdempotencySettings(
            "blog:create",
            command.Code,
            TimeSpan.FromMinutes(10)));
    }
}
```

`typeCode` 表示业务操作类型，`key` 应使用业务唯一标识，例如订单号、流水号、外部请求 ID。不要使用当前时间、随机数或过宽泛字段。

## 命令锁模板

当多个请求可能同时竞争同一资源时，为 Command 增加 `ICommandLock<TCommand>`。

```csharp
public class CreateBlogCommandLock : ICommandLock<CreateBlogCommand>
{
    public Task<CommandLockSettings> GetLockKeysAsync(
        CreateBlogCommand command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CommandLockSettings(command.Code, 10));
    }
}
```

锁 Key 要有明确业务含义，粒度不要过大。不要用分布式锁替代数据库唯一索引。

## 注册清单

MediatR pipeline：

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(SomeApplicationAssemblyMarker).Assembly);

    cfg.AddCommandLockBehavior()
       .AddIdempotentBehavior()
       .AddUnitOfWorkBehaviors();
});
```

如果业务系统没有任何幂等命令，可以不注册 `AddIdempotentBehavior()`：

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(SomeApplicationAssemblyMarker).Assembly);

    cfg.AddCommandLockBehavior()
       .AddUnitOfWorkBehaviors();
});
```

一旦注册了 `AddIdempotentBehavior()`，就必须注册幂等基础服务和 `IIdempotencyStore`。数据库版推荐直接使用 `AddDbIdempotency<TDbContext>(...)`。

UnitOfWork：

```csharp
builder.Services.AddUnitOfWork<AppDbContext>();
```

Repositories：

```csharp
builder.Services.AddWriteRepositories(typeof(SomeInfrastructureAssemblyMarker).Assembly);

builder.Services.AddReadableRepositories<ReadDbContext>(
    typeof(SomeDomainAssemblyMarker).Assembly,
    typeof(SomeInfrastructureAssemblyMarker).Assembly);
```

如果业务项目暂时只有一个 `AppDbContext`，也可以让默认查询仓储绑定同一个 DbContext：

```csharp
builder.Services.AddReadableRepositories<AppDbContext>(
    typeof(SomeDomainAssemblyMarker).Assembly,
    typeof(SomeInfrastructureAssemblyMarker).Assembly);
```

如果只需要注册默认通用查询仓储，不扫描自定义查询仓储：

```csharp
builder.Services.AddQueryRepositories<ReadDbContext>(
    typeof(SomeDomainAssemblyMarker).Assembly);
```

旧项目可以继续使用 `AddRepositories(...)` 注册写仓储；新代码优先使用 `AddWriteRepositories(...)` 表达读写分离。

Idempotency：

```csharp
builder.Services.AddDbIdempotency<AppDbContext>(
    new[] { typeof(SomeApplicationAssemblyMarker).Assembly },
    options =>
    {
        options.DefaultExpire = TimeSpan.FromDays(1);
        options.CleanupInterval = TimeSpan.FromMinutes(5);
        options.CleanupBatchSize = 200;
    });
```

如果业务项目自己实现幂等存储，可以使用基础注册：

```csharp
builder.Services.AddIdempotency(options =>
{
    options.DefaultExpire = TimeSpan.FromDays(1);
});

builder.Services.AddScoped<IIdempotencyStore, CustomIdempotencyStore>();
builder.Services.AddIdempotentCommands(typeof(SomeApplicationAssemblyMarker).Assembly);
```

Command Locks：

```csharp
builder.Services.AddCommandLocks(typeof(SomeApplicationAssemblyMarker).Assembly);
builder.Services.AddInMemoryDistributedLock();
```

Integration Events：

```csharp
builder.Services.AddIntegrationEvent();
```

如果项目使用 CAP/MySQL，并且 Integration Event 要和本地 EF transaction 协同，启用事务处理器：

```csharp
builder.Services.AddMySqlTransactionHandler();
```

## 完成前检查

提交功能前逐项确认：

- `Command`、`Query`、`DTO` 是否放在 `Application.Contracts`。
- 核心业务规则是否放在 Aggregate Root、Value Object 或 Domain Service。
- `CommandHandler` 是否只做用例编排，没有手写 transaction。
- 没有幂等命令时，是否避免无意义注册 `AddIdempotentBehavior()`。
- 使用 `AddIdempotentBehavior()` 时，是否同时注册了 `AddDbIdempotency<TDbContext>(...)` 或 `AddIdempotency()` + `IIdempotencyStore`。
- `QueryHandler` 是否优先使用默认 `IQueryRepository<TEntity>`，并使用 DTO projection 和 `AsNoTracking()`。
- `IntegrationEvent` 是否只包含稳定字段，没有领域实体。
- 写 Repository 是否只面向 Aggregate Root。
- 只有默认查询仓储不满足时，是否才定义自定义 Query Repository。
- EF Core mapping 是否包含 required、max length、index、soft-delete default 等数据库约束。
- async 调用是否传递 `CancellationToken`。
- 业务异常是否使用 `AppFriendlyException` 或 `ExceptionHelper`。
- 高风险业务规则是否有 Domain/Application 测试。
