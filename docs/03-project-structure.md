# 推荐项目结构

YSCore 推荐使用清晰的分层项目结构来组织企业应用代码。

这种结构适合单体、模块化单体和微服务应用。对于微服务系统，每个服务都可以使用同样的结构；对于模块化单体，可以在一个解决方案中按模块复用这套结构。

本章目标是回答一个最常见的问题：

> 代码应该放在哪里？

## 标准目录结构

推荐的解决方案结构如下：

```text
YourProject
 ├── src
 │   ├── YourProject.WebApi
 │   ├── YourProject.Application
 │   ├── YourProject.Application.Contracts
 │   ├── YourProject.Domain
 │   └── YourProject.Infrastructure
 └── tests
     ├── YourProject.WebApi.Tests
     ├── YourProject.Application.Tests
     ├── YourProject.Domain.Tests
     └── YourProject.Infrastructure.Tests
```

每一层都有明确职责。开发时应尽量避免把代码随意放置，尤其不要把业务规则写在 WebApi 或 Infrastructure 层。

## 项目职责概览

| 项目 | 职责 |
|---|---|
| `YourProject.WebApi` | HTTP API、认证授权、Swagger、中间件、应用启动配置 |
| `YourProject.Application` | CommandHandler、QueryHandler、应用服务、事件处理器、用例编排 |
| `YourProject.Application.Contracts` | DTO、Command、Query、应用服务接口、集成事件契约 |
| `YourProject.Domain` | 聚合根、实体、值对象、领域服务、领域事件、领域规则 |
| `YourProject.Infrastructure` | EF Core、仓储实现、CAP、Redis、第三方服务、数据库迁移 |
| `tests` | 各层测试项目 |

一个简单规则是：

```text
WebApi 负责接入
Application 负责用例
Domain 负责业务规则
Infrastructure 负责技术实现
Contracts 负责对外契约
```

## 推荐依赖关系

推荐依赖方向如下：

```text
YourProject.WebApi
 ├── YourProject.Application
 ├── YourProject.Application.Contracts
 └── YourProject.Infrastructure

YourProject.Application
 ├── YourProject.Application.Contracts
 └── YourProject.Domain

YourProject.Infrastructure
 ├── YourProject.Domain
 └── YourProject.Application.Contracts

YourProject.Domain
 └── YSCore.Base
```

可以用更简单的图表示：

```text
WebApi
  -> Application
  -> Infrastructure

Application
  -> Application.Contracts
  -> Domain

Infrastructure
  -> Domain
  -> Application.Contracts

Domain
  -> YSCore.Base
```

需要重点遵守以下规则：

- Domain 不依赖 WebApi。
- Domain 不依赖 Infrastructure。
- Domain 不依赖 EF Core、CAP、Redis、HTTP。
- Application 不直接依赖具体消息队列。
- Application 不应该包含数据库配置、Redis 配置或第三方 SDK 细节。
- Infrastructure 可以依赖 Domain 和 Contracts，用于实现持久化、消息、缓存和外部服务。
- WebApi 负责组合各层，并完成依赖注入注册。

## WebApi 层

`WebApi` 是应用的入口层，负责处理 HTTP 请求。

适合放在 WebApi 层的内容：

- Controller
- Minimal API Endpoint
- Swagger 配置
- 认证和授权配置
- 中间件注册
- CORS 配置
- 日志配置
- 健康检查
- 依赖注入组合
- 应用启动配置

不建议放在 WebApi 层的内容：

- 业务规则
- 领域模型
- 数据库访问逻辑
- 事务控制
- 消息发布细节
- Redis 操作细节

一个典型 Controller 应该保持很薄：

```csharp
[ApiController]
[Route("api/blogs")]
public class BlogController : ControllerBase
{
    private readonly IMediator _mediator;

    public BlogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public Task<bool> Create(CreateBlogCommand command, CancellationToken cancellationToken)
    {
        return _mediator.Send(command, cancellationToken);
    }
}
```

Controller 的职责是接收请求并发送 Command 或 Query，不应该直接写业务逻辑。

## Application.Contracts 层

`Application.Contracts` 是应用契约层。

它定义应用对外暴露的稳定契约，适合被 WebApi、其他服务或客户端 SDK 引用。

适合放在 Contracts 层的内容：

- DTO
- Command
- Query
- 分页请求模型
- 应用服务接口
- 集成事件契约
- 错误码常量
- 枚举契约

例如：

```text
YourProject.Application.Contracts
 ├── Blogs
 │   ├── CreateBlogCommand.cs
 │   ├── BlogDto.cs
 │   ├── GetBlogListQuery.cs
 │   └── IBlogAppService.cs
 └── IntegrationEvents
     └── BlogCreatedIntegrationEvent.cs
```

如果某个类型会被外部调用方、其他服务或 WebApi 层直接使用，通常可以放在 Contracts 层。

需要注意：

- Contracts 层应保持稳定。
- 不要把领域实体放到 Contracts 层。
- 集成事件应使用稳定字段，不要直接暴露聚合根。
- Command 可以放在 Contracts 层，便于 WebApi 直接接收请求。

## Application 层

`Application` 是应用层，负责组织业务用例。

它不直接表达 HTTP，也不直接关心数据库技术细节，而是负责协调领域模型、仓储、事件和外部服务接口。

适合放在 Application 层的内容：

- CommandHandler
- QueryHandler
- 应用服务实现
- 领域事件处理器
- 集成事件处理器
- 幂等配置
- 命令锁配置
- 应用层权限检查
- 业务流程编排

推荐目录结构：

```text
YourProject.Application
 ├── Blogs
 │   ├── CreateBlogCommandHandler.cs
 │   ├── GetBlogListQueryHandler.cs
 │   ├── CreateBlogCommandIdempotent.cs
 │   ├── CreateBlogCommandLock.cs
 │   └── Events
 │       └── BlogCreatedDomainEventHandler.cs
 └── IntegrationEventHandlers
     └── InventoryChangedIntegrationEventHandler.cs
```

Application 层的 Handler 通常应该这样工作：

```text
接收 Command
 -> 加载聚合根
 -> 调用领域行为
 -> 保存或更新聚合
 -> 返回结果
```

CommandHandler 中不推荐做这些事：

- 手动开启事务
- 手动提交事务
- 直接调用消息队列 SDK
- 把大量业务规则写成过程式代码
- 直接返回 EF Core 实体给 API

YSCore 推荐通过 UnitOfWork Pipeline 统一处理事务。

## Domain 层

`Domain` 是领域层，是业务规则最核心的位置。

适合放在 Domain 层的内容：

- 聚合根
- 实体
- 值对象
- 领域服务
- 领域事件
- 枚举对象
- 业务规则
- 领域异常

推荐目录结构：

```text
YourProject.Domain
 ├── Blogs
 │   ├── Blog.cs
 │   ├── Post.cs
 │   ├── BlogCode.cs
 │   ├── BlogStatus.cs
 │   └── Events
 │       └── BlogCreatedDomainEvent.cs
 └── Shared
     └── Money.cs
```

领域层应该尽量保持纯净。

不建议在 Domain 层引用：

- ASP.NET Core
- EF Core
- CAP
- Redis
- HTTP Client
- 第三方 SDK
- Infrastructure 项目

领域模型应该通过方法表达业务行为，而不是让外部随意修改属性。

推荐：

```csharp
public void ChangeName(string name)
{
    if (string.IsNullOrWhiteSpace(name))
    {
        throw new AppFriendlyException("博客名称不能为空");
    }

    Name = name;
}
```

不推荐：

```csharp
blog.Name = request.Name;
```

领域层的核心目标是保护业务规则，而不是只作为数据库表结构的映射对象。

## Infrastructure 层

`Infrastructure` 是基础设施层，负责实现技术细节。

适合放在 Infrastructure 层的内容：

- EF Core DbContext
- EntityTypeConfiguration
- Repository 实现
- 数据库迁移
- CAP 配置
- Redis 实现
- 分布式锁 Provider 配置
- 第三方服务实现
- 文件存储
- 邮件发送
- 短信发送
- 外部 HTTP API Client

推荐目录结构：

```text
YourProject.Infrastructure
 ├── EntityFrameworkCore
 │   ├── AppDbContext.cs
 │   ├── Configurations
 │   │   └── BlogEntityTypeConfiguration.cs
 │   ├── Repositories
 │   │   └── BlogRepository.cs
 │   └── Migrations
 ├── Redis
 ├── Cap
 ├── ExternalServices
 └── DependencyInjection
     └── ServiceCollectionExtensions.cs
```

Infrastructure 层可以引用 Domain，因为它需要持久化领域对象。

Infrastructure 层也可以引用 Application.Contracts，因为它可能需要实现某些应用接口，或者处理集成事件契约。

## tests 目录

推荐为每一层建立测试项目：

```text
tests
 ├── YourProject.WebApi.Tests
 ├── YourProject.Application.Tests
 ├── YourProject.Domain.Tests
 └── YourProject.Infrastructure.Tests
```

不同测试项目关注点不同。

| 测试项目 | 关注点 |
|---|---|
| `Domain.Tests` | 聚合根、值对象、领域规则、领域事件产生 |
| `Application.Tests` | CommandHandler、QueryHandler、用例流程、幂等规则 |
| `Infrastructure.Tests` | Repository、DbContext、CAP、Redis、外部服务 |
| `WebApi.Tests` | API 路由、认证授权、统一返回值、集成测试 |

建议优先编写 Domain 和 Application 测试。

因为这两层最能保护业务规则，也最不容易受到基础设施变化影响。

## 按功能组织目录

在 Application、Contracts、Domain 层，推荐按业务功能组织目录，而不是按技术类型组织目录。

推荐：

```text
Application
 ├── Blogs
 │   ├── CreateBlogCommandHandler.cs
 │   ├── UpdateBlogCommandHandler.cs
 │   └── GetBlogListQueryHandler.cs
 └── Posts
     ├── CreatePostCommandHandler.cs
     └── PublishPostCommandHandler.cs
```

不推荐：

```text
Application
 ├── Commands
 ├── Queries
 ├── Handlers
 └── Services
```

按功能组织代码，更适合企业项目长期维护。

当业务增长后，开发者可以很快找到某个业务模块的所有用例，而不是在 Commands、Handlers、Services 之间来回跳转。

## 一个 Blog 模块示例

下面是一个更完整的 Blog 模块目录示例：

```text
YourProject.Application.Contracts
 └── Blogs
     ├── CreateBlogCommand.cs
     ├── UpdateBlogCommand.cs
     ├── BlogDto.cs
     └── GetBlogListQuery.cs

YourProject.Application
 └── Blogs
     ├── CreateBlogCommandHandler.cs
     ├── UpdateBlogCommandHandler.cs
     ├── GetBlogListQueryHandler.cs
     ├── CreateBlogCommandIdempotent.cs
     ├── CreateBlogCommandLock.cs
     └── Events
         └── BlogCreatedDomainEventHandler.cs

YourProject.Domain
 └── Blogs
     ├── Blog.cs
     ├── Post.cs
     └── Events
         └── BlogCreatedDomainEvent.cs

YourProject.Infrastructure
 └── EntityFrameworkCore
     ├── AppDbContext.cs
     ├── Configurations
     │   └── BlogEntityTypeConfiguration.cs
     └── Repositories
         └── BlogRepository.cs

YourProject.WebApi
 └── Controllers
     └── BlogController.cs
```

这个结构的好处是：

- API 层保持轻量
- 用例集中在 Application
- 业务规则集中在 Domain
- 技术实现集中在 Infrastructure
- 对外契约集中在 Contracts

## 单体、模块化单体和微服务中的使用方式

### 单体应用

单体应用可以使用一套完整分层结构：

```text
YourProject.WebApi
YourProject.Application
YourProject.Application.Contracts
YourProject.Domain
YourProject.Infrastructure
```

这种方式适合中小型企业系统，也方便未来继续演进。

### 模块化单体

模块化单体可以按模块划分：

```text
src
 ├── Order
 │   ├── Order.Application
 │   ├── Order.Application.Contracts
 │   ├── Order.Domain
 │   └── Order.Infrastructure
 ├── Inventory
 │   ├── Inventory.Application
 │   ├── Inventory.Application.Contracts
 │   ├── Inventory.Domain
 │   └── Inventory.Infrastructure
 └── YourProject.WebApi
```

这种方式适合业务边界较清晰，但暂时不需要拆成独立服务的系统。

### 微服务

微服务中，每个服务都可以拥有自己的分层结构：

```text
OrderService
 ├── OrderService.WebApi
 ├── OrderService.Application
 ├── OrderService.Application.Contracts
 ├── OrderService.Domain
 └── OrderService.Infrastructure

InventoryService
 ├── InventoryService.WebApi
 ├── InventoryService.Application
 ├── InventoryService.Application.Contracts
 ├── InventoryService.Domain
 └── InventoryService.Infrastructure
```

服务之间不应该直接共享数据库，也不应该直接引用彼此的 Domain。

服务之间推荐通过：

- HTTP API
- IntegrationEvent
- 契约包
- 消息队列

进行协作。

## 常见错误

### 把业务逻辑写在 Controller 中

不推荐：

```csharp
[HttpPost]
public async Task<bool> Create(CreateBlogCommand command)
{
    var blog = new Blog(command.Name, command.Code);
    await _dbContext.AddAsync(blog);
    await _dbContext.SaveChangesAsync();
    return true;
}
```

推荐：

```csharp
[HttpPost]
public Task<bool> Create(CreateBlogCommand command, CancellationToken cancellationToken)
{
    return _mediator.Send(command, cancellationToken);
}
```

### Domain 层依赖 Infrastructure

不推荐让领域模型依赖 DbContext、Redis 或消息队列。

领域层应该描述业务规则，而不是描述技术实现。

### 直接把实体作为 API 返回值

不推荐：

```csharp
public Task<Blog> Get(long id)
```

推荐：

```csharp
public Task<BlogDto> Get(long id)
```

API 返回值应该使用 DTO，避免把领域模型结构直接暴露给外部调用方。

### 集成事件直接使用领域实体

不推荐：

```csharp
public record BlogCreatedIntegrationEvent(Blog Blog) : IIntegrationEvent;
```

推荐：

```csharp
public record BlogCreatedIntegrationEvent(long BlogId, string Name, string Code) : IIntegrationEvent;
```

集成事件是跨服务契约，应保持稳定、明确、可版本化。

## 小结

YSCore 推荐使用分层结构来保持企业应用的长期可维护性。

最重要的原则是：

- WebApi 只做接入。
- Application 组织用例。
- Application.Contracts 定义契约。
- Domain 保护业务规则。
- Infrastructure 实现技术细节。
- tests 保护业务行为和关键集成。

下一章将进入基础开发指南，介绍 Command、CommandHandler、Repository、UnitOfWork、领域事件和集成事件的日常使用方式。

