# 快速开始

本章将带你创建一个最小可运行的 NexDDD 应用，并跑通框架最核心的业务开发流程：

```text
HTTP API
 -> Command
 -> CommandHandler
 -> EF Core
 -> UnitOfWork
 -> DomainEvent
```

本章不会展开所有高级能力，而是优先帮助你理解：在 NexDDD 中，一个标准业务用例应该如何编写。

## 环境准备

开始之前，请确保本地已经安装：

- .NET 8 SDK 或更高版本
- Visual Studio、Rider 或 Visual Studio Code
- 一个 EF Core 支持的关系型数据库，例如 MySQL、SQL Server 或 PostgreSQL
- Redis、Kafka、RabbitMQ 在本章中不是必需项

第一个应用只需要使用 ASP.NET Core、MediatR、EF Core 和 NexDDD 基础能力。

## 使用 DDD 项目模板

公司内部新项目推荐先安装仓库中的 NexDDD DDD 模板：

```bash
dotnet new install ./templates/NexDDD.Ddd.Template
dotnet new tpl -n YourCompany.YourService
```

该命令会生成 `Application.Contracts`、`Application`、`Domain`、`Infrastructure`、`WebApi` 和 `Domain.Tests` 项目，并提供一个贯通 Command、Query、聚合根、Domain Event、Repository、EF Core 与 UnitOfWork 的商品示例。

模板也可以打包并发布到公司 NuGet 源。安装、打包和版本参数说明见 [`templates/NexDDD.Ddd.Template/README.md`](../templates/NexDDD.Ddd.Template/README.md)。

## 创建 Web API 项目

创建一个 ASP.NET Core Web API 项目：

```bash
dotnet new webapi -n YourProject.WebApi
```

如果你直接引用 NexDDD 源码，可以添加项目引用：

```bash
dotnet add YourProject.WebApi reference path/to/NexDDD.Core.csproj
dotnet add YourProject.WebApi reference path/to/NexDDD.Provider.EntityFrameworkCore.csproj
```

如果你通过 NuGet 包使用 NexDDD，可以安装对应包：

```bash
dotnet add package NexDDD.Core
dotnet add package NexDDD.Provider.EntityFrameworkCore
```

实际项目需要安装哪些包，取决于你启用了哪些能力。最小应用通常从核心包和 EF Core Provider 开始。

## 注册 NexDDD

在 `Program.cs` 中注册框架基础服务：

```csharp
using System.Reflection;
using MediatR;
using NexDDD.Core.App.Extensions;
using NexDDD.Provider.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNexDDD();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
    cfg.AddUnitOfWorkBehaviors();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

`AddNexDDD` 会注册框架基础能力，例如数据验证、友好异常和统一返回值。

`AddUnitOfWorkBehaviors` 会注册命令事务管线。启用后，CommandHandler 中通常不需要手动开启事务、提交事务或调用 `SaveChangesAsync`。

## 创建第一个 Command

在 NexDDD 中，Command 用来表达一次业务意图。

例如，“创建博客”可以定义为一个 Command：

```csharp
using NexDDD.Core.Events;

namespace YourProject.Application.Blogs;

public record CreateBlogCommand(string Name, string Code) : ICommand<bool>;
```

Command 应保持简单，只描述“要做什么”，不要在 Command 中编写业务逻辑。

## 创建 CommandHandler

创建命令处理器：

```csharp
using YourProject.Domain.Blogs;
using YourProject.Infrastructure.EntityFrameworkCore;
using NexDDD.Core.Events;

namespace YourProject.Application.Blogs;

public class CreateBlogCommandHandler : ICommandHandler<CreateBlogCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public CreateBlogCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = new Blog(request.Name, request.Code);

        await _dbContext.AddAsync(blog, cancellationToken);

        return true;
    }
}
```

注意，Handler 中没有调用 `SaveChangesAsync`。

NexDDD 推荐以 Command 作为事务边界。CommandHandler 执行成功后，UnitOfWork 管线会自动保存实体变更并提交事务。

## 创建实体和聚合根

创建一个简单的聚合根：

```csharp
using Yitter.IdGenerator;
using NexDDD.Core.DatabaseAccessor;

namespace YourProject.Domain.Blogs;

public class Blog : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }

    public string Code { get; private set; }

    protected Blog()
    {
    }

    public Blog(string name, string code)
    {
        Id = YitIdHelper.NextId();
        Name = name;
        Code = code;
        CreateTime = DateTime.UtcNow;
    }
}
```

`BaseEntity` 提供了常用实体字段：

- `Id`
- `CreateTime`
- `LastModifyTime`
- `CreateUserId`
- `CreateUserName`
- `IsDelete`

`IAggregateRoot` 用来标记聚合根。NexDDD 的仓储接口默认面向聚合根设计。

## 创建 DbContext

创建 EF Core DbContext：

```csharp
using Microsoft.EntityFrameworkCore;
using YourProject.Domain.Blogs;
using NexDDD.Provider.EntityFrameworkCore;

namespace YourProject.Infrastructure.EntityFrameworkCore;

public class AppDbContext : AppDbContextBase
{
    public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    public DbSet<Blog> Blogs => Set<Blog>();
}
```

注册 DbContext 和 UnitOfWork：

```csharp
using Microsoft.EntityFrameworkCore;
using NexDDD.Provider.EntityFrameworkCore.Extensions;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddUnitOfWork<AppDbContext>();
```

添加数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "Default": "server=localhost;port=3306;database=nexddd_demo;user=root;password=your_password;CharSet=utf8mb4;"
  }
}
```

完成这一步后，CommandHandler 可以注入 `AppDbContext`，并由 NexDDD 的 UnitOfWork 管线统一提交事务。

## 创建 API 接口

可以通过 Controller 暴露 Command：

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using YourProject.Application.Blogs;

namespace YourProject.WebApi.Controllers;

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
    public async Task<bool> Create(CreateBlogCommand command, CancellationToken cancellationToken)
    {
        return await _mediator.Send(command, cancellationToken);
    }
}
```

API 层应该保持轻量，只负责接收 HTTP 请求并发送 Command。

业务规则应放在 Application 层和 Domain 层，而不是写在 Controller 中。

## 添加领域事件

领域事件用于表达领域中已经发生的事实。

创建一个领域事件：

```csharp
using NexDDD.Core.DomainEvent;

namespace YourProject.Domain.Blogs.Events;

public record BlogCreatedDomainEvent(long BlogId, string Name, string Code) : IDomainEvent;
```

在聚合根中产生事件：

```csharp
using YourProject.Domain.Blogs.Events;

public Blog(string name, string code)
{
    Id = YitIdHelper.NextId();
    Name = name;
    Code = code;
    CreateTime = DateTime.UtcNow;

    AddDomainEvent(new BlogCreatedDomainEvent(Id, Name, Code));
}
```

创建领域事件处理器：

```csharp
using Microsoft.Extensions.Logging;
using YourProject.Domain.Blogs.Events;
using NexDDD.Core.DomainEvent;

namespace YourProject.Application.Blogs.Events;

public class BlogCreatedDomainEventHandler : IDomainEventHandler<BlogCreatedDomainEvent>
{
    private readonly ILogger<BlogCreatedDomainEventHandler> _logger;

    public BlogCreatedDomainEventHandler(ILogger<BlogCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BlogCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Blog created: {BlogId}, {Name}", notification.BlogId, notification.Name);
        return Task.CompletedTask;
    }
}
```

领域事件不需要手动发布。

Command 执行成功后，UnitOfWork 会保存实体变更，并自动分发实体中记录的领域事件。

## 启用命令幂等

对于可能重复提交的操作，可以启用命令幂等。

创建幂等规则：

```csharp
using NexDDD.Core.Idempotency;

namespace YourProject.Application.Blogs;

public class CreateBlogCommandIdempotent : IIdempotentCommand<CreateBlogCommand>
{
    public Task<IdempotencySettings> GetIdempotencyAsync(
        CreateBlogCommand command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new IdempotencySettings(
                typeCode: "blog:create",
                key: command.Code,
                expireIn: TimeSpan.FromMinutes(10)));
    }
}
```

注册幂等管线：

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
    cfg.AddIdempotentBehavior();
    cfg.AddUnitOfWorkBehaviors();
});
```

注册幂等命令和幂等存储。使用 EF Core 存储时：

```csharp
using NexDDD.Provider.EntityFrameworkCore.Idempotency;

builder.Services.AddDbIdempotency<AppDbContext>(
    new[] { Assembly.GetExecutingAssembly() },
    options =>
    {
        options.DefaultExpire = TimeSpan.FromDays(1);
        options.CleanupInterval = TimeSpan.FromMinutes(5);
        options.CleanupBatchSize = 200;
    });
```

启用后，相同幂等 Key 的重复请求会被框架识别并阻止。

## 启用命令锁

对于不能并发执行的业务操作，可以启用命令锁。

创建命令锁规则：

```csharp
using NexDDD.Core.CommandLocks;

namespace YourProject.Application.Blogs;

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

注册命令锁管线：

```csharp
using NexDDD.Core.CommandLocks;
using NexDDD.Core.DistributedLocks;

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
    cfg.AddCommandLockBehavior();
    cfg.AddIdempotentBehavior();
    cfg.AddUnitOfWorkBehaviors();
});

builder.Services.AddCommandLocks(Assembly.GetExecutingAssembly());
```

本地开发可以使用内存锁：

```csharp
builder.Services.AddInMemoryDistributedLock();
```

生产环境或多实例部署时，建议使用 Redis 分布式锁 Provider。

## 推荐的管线顺序

大多数应用推荐使用以下顺序：

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());

    cfg.AddCommandLockBehavior()
       .AddIdempotentBehavior()
       .AddUnitOfWorkBehaviors();
});
```

执行过程可以理解为：

1. 获取命令锁
2. 检查幂等
3. 开启 UnitOfWork 事务
4. 执行 CommandHandler
5. 保存实体并分发领域事件
6. 标记幂等记录为已完成
7. 提交事务

## 本章完成了什么

现在你已经完成了一个最小 NexDDD 业务流程：

```text
POST /api/blogs
 -> CreateBlogCommand
 -> CreateBlogCommandHandler
 -> Blog 聚合根
 -> BlogCreatedDomainEvent
 -> UnitOfWork 事务
 -> 数据库提交
```

你也初步了解了以下能力：

- Command
- CommandHandler
- EF Core DbContext
- UnitOfWork
- 领域事件
- 命令幂等
- 命令锁

## 下一步

下一章将介绍 NexDDD 推荐的企业项目结构，包括 WebApi、Application、Application.Contracts、Domain、Infrastructure 和 tests 应该如何划分职责。

