# 基础开发指南

本章介绍 YSCore 日常开发中最常用的基础模型和开发方式。

如果你已经完成了“快速开始”，那么本章会进一步解释：为什么推荐这样写，以及在真实项目中应该如何组织 Command、Handler、仓储、UnitOfWork、领域事件和集成事件。

本章重点关注以下内容：

- Command 与 CommandHandler
- Query 与查询模型
- 仓储
- UnitOfWork
- 领域事件
- 集成事件
- 常见开发约定

## Command

Command 表示一次业务意图。

例如：

```csharp
public record CreateOrderCommand(long CustomerId, List<CreateOrderItemDto> Items) : ICommand<long>;
```

这个 Command 表达的是：

> 创建一笔订单，并返回订单 ID。

Command 只应该描述“要做什么”，不应该包含业务逻辑。

推荐：

```csharp
public record CreateBlogCommand(string Name, string Code) : ICommand<bool>;
```

不推荐：

```csharp
public class CreateBlogCommand : ICommand<bool>
{
    public string Name { get; set; }

    public string Code { get; set; }

    public Blog ToEntity()
    {
        return new Blog(Name, Code);
    }
}
```

Command 中不应该负责创建实体或执行业务规则。它只是应用层用例的输入模型。

## Command 命名规范

Command 建议使用动词开头，表达清晰的业务动作。

推荐命名：

- `CreateBlogCommand`
- `UpdateBlogCommand`
- `DeleteBlogCommand`
- `PublishPostCommand`
- `CancelOrderCommand`
- `PayOrderCommand`
- `DeductInventoryCommand`

不推荐命名：

- `BlogCommand`
- `BlogRequest`
- `SaveDataCommand`
- `HandleOrderCommand`
- `DoSomethingCommand`

好的 Command 名称应该让开发者一眼看出业务动作。

## Command 返回值

Command 的返回值应该保持简单。

常见返回值包括：

- `bool`
- `long`
- `Guid`
- `string`
- 简单 DTO

例如：

```csharp
public record CreateBlogCommand(string Name, string Code) : ICommand<long>;
```

表示创建成功后返回 Blog ID。

对于复杂查询结果，不建议通过 Command 返回。复杂数据读取更适合使用 Query。

## CommandHandler

CommandHandler 负责处理一个业务用例。

典型结构如下：

```csharp
public class CreateBlogCommandHandler : ICommandHandler<CreateBlogCommand, long>
{
    private readonly IBlogRepository _blogRepository;

    public CreateBlogCommandHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<long> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = new Blog(request.Name, request.Code);

        await _blogRepository.AddAsync(blog, cancellationToken);

        return blog.Id;
    }
}
```

CommandHandler 通常负责：

- 接收 Command
- 加载聚合根
- 调用领域对象的业务方法
- 调用仓储添加或更新聚合
- 返回处理结果

CommandHandler 不应该负责：

- 手动开启事务
- 手动提交事务
- 手动分发领域事件
- 编写 HTTP 逻辑
- 直接调用消息队列 SDK
- 直接操作 Redis 完成业务规则

事务、领域事件分发和幂等完成状态由框架管线统一处理。

## CommandHandler 与领域模型

CommandHandler 不应该把所有业务规则都写成过程式代码。

不推荐：

```csharp
public async Task<bool> Handle(UpdateBlogNameCommand request, CancellationToken cancellationToken)
{
    var blog = await _blogRepository.GetAsync(request.Id, cancellationToken);

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        throw new AppFriendlyException("博客名称不能为空");
    }

    blog.Name = request.Name;

    await _blogRepository.UpdateAsync(blog, cancellationToken);

    return true;
}
```

推荐：

```csharp
public async Task<bool> Handle(UpdateBlogNameCommand request, CancellationToken cancellationToken)
{
    var blog = await _blogRepository.GetAsync(request.Id, cancellationToken);

    if (blog == null)
    {
        throw new AppFriendlyException("博客不存在");
    }

    blog.ChangeName(request.Name);

    await _blogRepository.UpdateAsync(blog, cancellationToken);

    return true;
}
```

领域规则应该尽量放到聚合根、实体、值对象或领域服务中。

Application 层负责用例编排，Domain 层负责业务规则。

## Query

Query 表示一次数据读取请求。

YSCore 当前核心重点是 Command 管线，但在企业应用中，仍然建议使用 Query 来组织复杂查询。

推荐命名：

- `GetBlogQuery`
- `GetBlogListQuery`
- `GetOrderDetailQuery`
- `SearchCustomersQuery`

Query 通常返回 DTO，而不是领域实体。

例如：

```csharp
public record GetBlogQuery(long Id) : IRequest<BlogDto>;
```

QueryHandler 示例：

```csharp
public class GetBlogQueryHandler : IRequestHandler<GetBlogQuery, BlogDto>
{
    private readonly AppDbContext _dbContext;

    public GetBlogQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BlogDto> Handle(GetBlogQuery request, CancellationToken cancellationToken)
    {
        var blog = await _dbContext.Blogs
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BlogDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (blog == null)
        {
            throw new AppFriendlyException("博客不存在");
        }

        return blog;
    }
}
```

读操作建议：

- 返回 DTO
- 使用 `AsNoTracking`
- 使用投影查询
- 避免直接返回实体
- 避免为了查询强行加载完整聚合

## 仓储

仓储用于访问聚合根。

YSCore 的仓储接口要求实体必须是聚合根：

```csharp
public interface IYsRepository<TEntity> : IReadableRepository<TEntity>
    where TEntity : notnull, Entity, IAggregateRoot
{
}
```

这意味着仓储不应该随意面向任意实体，而应该围绕聚合根设计。

例如：

```csharp
public interface IBlogRepository : IYsRepository<Blog, long>
{
    Task<Blog?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
```

仓储实现可以放在 Infrastructure 层：

```csharp
public class BlogRepository : YsRepositoryBase<Blog, long, AppDbContext>, IBlogRepository
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

仓储适合用于：

- 加载聚合根
- 新增聚合根
- 更新聚合根
- 删除聚合根
- 封装与聚合相关的查询

仓储不适合用于：

- 跨多个聚合的大型报表查询
- 复杂列表筛选和投影
- 直接暴露数据库表结构
- 绕过聚合根修改子实体

复杂读模型可以直接使用 QueryHandler + DbContext 投影查询。

## UnitOfWork

UnitOfWork 用于管理一次业务操作中的数据变更和事务提交。

YSCore 中的核心接口是：

```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
```

其中 `SaveEntitiesAsync` 不只是保存数据库变更，还会协调领域事件分发。

在 Command 管线中，YSCore 会自动执行：

```text
开启事务
 -> 执行 CommandHandler
 -> SaveEntitiesAsync
 -> 标记幂等完成
 -> 提交事务
```

因此，CommandHandler 中通常不需要写：

```csharp
await _dbContext.SaveChangesAsync(cancellationToken);
```

也不需要写：

```csharp
await using var transaction = await _dbContext.Database.BeginTransactionAsync();
```

这些由 UnitOfWork 管线统一处理。

## UnitOfWork 的事务边界

YSCore 推荐一个 Command 对应一个事务边界。

例如：

```text
CreateOrderCommand
 -> 创建订单
 -> 扣减本地库存预占记录
 -> 产生 OrderCreatedDomainEvent
 -> 保存实体
 -> 分发领域事件
 -> 提交事务
```

如果 CommandHandler 执行过程中发生异常，UnitOfWork 会回滚事务。

这可以避免业务代码中到处散落事务控制逻辑。

## 领域事件

领域事件用于表达领域中已经发生的事实。

例如：

- `BlogCreatedDomainEvent`
- `OrderPaidDomainEvent`
- `InventoryDeductedDomainEvent`

领域事件应该由聚合根或实体产生：

```csharp
public Blog(string name, string code)
{
    Id = YitIdHelper.NextId();
    Name = name;
    Code = code;

    AddDomainEvent(new BlogCreatedDomainEvent(Id, Name, Code));
}
```

领域事件处理器：

```csharp
public class BlogCreatedDomainEventHandler : IDomainEventHandler<BlogCreatedDomainEvent>
{
    public Task Handle(BlogCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

领域事件适合处理当前服务内部的副作用，例如：

- 写入审计日志
- 创建本地通知
- 更新本地派生数据
- 发布集成事件

领域事件不适合直接作为服务间契约。

如果其他服务需要感知这个事件，应转换为集成事件。

## 领域事件与事务

YSCore 的 EF Core Provider 会在 `SaveEntitiesAsync` 中分发领域事件。

典型顺序是：

```text
保存实体变更
 -> 收集领域事件
 -> 清理实体上的领域事件
 -> 发布领域事件
 -> 提交事务
```

如果领域事件处理器抛出异常，事务会回滚。

这意味着领域事件处理器适合处理必须与当前业务操作保持一致的本地副作用。

如果某个副作用不应该影响当前事务，例如调用外部系统，建议通过集成事件异步处理。

## 集成事件

集成事件用于服务之间通信。

它表示当前服务希望对外发布的业务事实。

例如：

```csharp
public record BlogCreatedIntegrationEvent(long BlogId, string Name, string Code) : IIntegrationEvent;
```

发布集成事件：

```csharp
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

消费集成事件：

```csharp
public class BlogCreatedIntegrationEventHandler
    : IIntegrationEventHandler<BlogCreatedIntegrationEvent>
{
    public Task HandleAsync(
        IntegrationEventHandlerContext eventData,
        CancellationToken cancellationToken = default)
    {
        var integrationEvent = (BlogCreatedIntegrationEvent)eventData.Source;

        return Task.CompletedTask;
    }
}
```

业务代码依赖的是 `IIntegrationEventPublisher` 和 `IIntegrationEventHandler<T>`，不是 CAP 的原生 API。

CAP 只是 YSCore 的一种 Provider 实现。

## 领域事件与集成事件的区别

| 对比项 | 领域事件 | 集成事件 |
|---|---|---|
| 使用范围 | 当前服务内部 | 服务之间 |
| 传输方式 | MediatR 进程内发布 | CAP / MQ |
| 是否是对外契约 | 否 | 是 |
| 是否可以包含领域对象 | 可以谨慎使用 | 不推荐 |
| 失败影响 | 通常影响当前事务 | 通常通过重试和补偿处理 |
| 命名示例 | `OrderPaidDomainEvent` | `OrderPaidIntegrationEvent` |

一个常见模式是：

```text
聚合根产生领域事件
 -> 领域事件处理器处理本地副作用
 -> 必要时发布集成事件
 -> 其他服务消费集成事件
```

## DTO

DTO 用于应用层和外部调用方之间传递数据。

DTO 不应该包含业务逻辑。

例如：

```csharp
public class BlogDto
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }
}
```

DTO 适合放在 `Application.Contracts` 层。

建议：

- API 返回 DTO，不返回实体
- Query 返回 DTO
- 集成事件使用独立事件契约，不直接复用实体
- DTO 字段应面向调用方需要，而不是完全复制数据库表

## 应用服务

YSCore 推荐优先使用 Command / Query 组织业务用例。

但在某些场景中，也可以定义应用服务接口，例如：

```csharp
public interface IBlogAppService
{
    Task<long> CreateAsync(CreateBlogCommand command, CancellationToken cancellationToken = default);

    Task<BlogDto> GetAsync(long id, CancellationToken cancellationToken = default);
}
```

应用服务适合用于：

- 对外暴露一组稳定业务能力
- 封装多个 Command / Query
- 为客户端 SDK 提供统一入口
- 兼容传统 Service 风格项目

如果项目已经全面使用 MediatR，应用服务可以作为可选组织方式。

## 常见开发流程

一个标准写操作通常按以下步骤开发：

```text
1. 在 Contracts 层定义 Command
2. 在 Domain 层定义或修改聚合根
3. 在 Domain 层定义领域事件
4. 在 Application 层实现 CommandHandler
5. 在 Application 层实现领域事件处理器
6. 在 Infrastructure 层配置实体映射和仓储
7. 在 WebApi 层暴露接口
8. 编写 Domain 或 Application 测试
```

一个标准读操作通常按以下步骤开发：

```text
1. 在 Contracts 层定义 Query 和 DTO
2. 在 Application 层实现 QueryHandler
3. 使用 DbContext 或只读仓储查询数据
4. 使用投影返回 DTO
5. 在 WebApi 层暴露接口
```

## 开发约定

推荐遵守以下约定：

- 一个 Command 表示一个明确业务动作。
- 一个 CommandHandler 只处理一个 Command。
- CommandHandler 不手动提交事务。
- 聚合根负责保护业务规则。
- 领域事件由聚合根或实体产生。
- 领域事件用于服务内部。
- 集成事件用于服务之间。
- 集成事件不直接暴露领域实体。
- Repository 面向聚合根。
- Query 返回 DTO，不返回实体。
- WebApi 层保持轻量。
- Infrastructure 层承载技术实现。

## 小结

YSCore 的基础开发模型可以概括为：

```text
Command 表达业务意图
CommandHandler 编排业务用例
Domain 保护业务规则
Repository 访问聚合根
UnitOfWork 管理事务和领域事件
IntegrationEvent 完成服务间通信
```

掌握这些基础概念后，就可以开始使用 YSCore 编写大多数企业应用场景。

下一章将介绍 YSCore 的常用能力，包括幂等、分布式锁、分布式事务与 Outbox、多租户、权限、异常处理和统一返回值。

