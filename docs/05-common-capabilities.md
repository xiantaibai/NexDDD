# 常用能力

YSCore 除了提供 DDD、CQRS、仓储和 UnitOfWork 等基础开发模型外，还内置了一组企业应用常用能力。

这些能力主要用于解决真实项目中的工程问题：

- 重复请求
- 跨节点并发
- 微服务事务一致性
- 服务间事件通信
- 多租户隔离
- 权限控制
- 统一异常
- 统一返回值

本章会先介绍这些能力的使用场景，再给出最常见的接入方式。

## 初始化框架配置

部分能力需要通过 `AppHelper` 读取应用配置和根服务，例如多语言、友好异常配置、统一返回值配置等。

因此，推荐在 `Program.cs` 最开始调用 `InitYsCore()`：

```csharp
using YSCore.Base.App.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .InitYsCore();
```

然后再注册框架服务：

```csharp
builder.Services.AddYsCore();
```

`InitYsCore` 负责初始化框架应用上下文，例如配置对象、宿主环境、根服务访问能力和 `HttpContextAccessor`。

`AddYsCore` 负责注册框架基础服务，例如数据验证、友好异常和统一返回值。

## 幂等

幂等用于处理重复请求或重复执行问题。

在企业系统中，重复请求很常见：

- 用户重复点击提交按钮
- 前端请求超时后重试
- 网关或客户端自动重试
- 消息队列重复投递
- 第三方平台重复回调

如果没有幂等保护，可能会出现重复创建订单、重复扣减库存、重复发放权益等问题。

YSCore 提供命令级幂等能力。一个 Command 可以通过实现 `IIdempotentCommand<TCommand>` 来声明幂等规则。

```csharp
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

`IdempotencySettings` 包含三个核心信息：

| 参数 | 说明 |
|---|---|
| `typeCode` | 业务操作类型，例如 `order:create`、`payment:callback` |
| `key` | 幂等 Key，通常来自业务唯一标识 |
| `expireIn` | 幂等记录有效期 |

推荐的幂等 Key：

- 订单号
- 支付流水号
- 外部回调单号
- 业务唯一编码
- 客户端请求 ID

不推荐的幂等 Key：

- 当前时间
- 随机数
- 用户名
- 过宽泛的业务类型

注册幂等管线：

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
    cfg.AddIdempotentBehavior();
    cfg.AddUnitOfWorkBehaviors();
});
```

使用 EF Core 存储幂等记录：

```csharp
builder.Services.AddDbIdempotency<AppDbContext>(
    new[] { Assembly.GetExecutingAssembly() },
    options =>
    {
        options.DefaultExpire = TimeSpan.FromDays(1);
        options.CleanupInterval = TimeSpan.FromMinutes(5);
        options.CleanupBatchSize = 200;
    });
```

幂等执行过程：

```text
请求进入
 -> 计算幂等 Key
 -> 尝试写入 Processing 记录
 -> 执行业务 Command
 -> UnitOfWork 提交
 -> 标记为 Completed
```

如果业务执行失败，框架会尝试移除幂等记录，允许后续重新提交。

## 分布式锁

分布式锁用于处理多实例部署下的并发竞争。

例如：

- 同一个订单不能被多个节点同时取消
- 同一个库存项不能被多个请求同时扣减
- 同一个任务不能被多个实例同时执行
- 同一个业务编码不能被并发创建

YSCore 提供命令级锁能力。一个 Command 可以通过实现 `ICommandLock<TCommand>` 来声明锁规则。

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

注册命令锁：

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());

    cfg.AddCommandLockBehavior()
       .AddIdempotentBehavior()
       .AddUnitOfWorkBehaviors();
});

builder.Services.AddCommandLocks(Assembly.GetExecutingAssembly());
```

本地开发可以使用内存锁：

```csharp
builder.Services.AddInMemoryDistributedLock();
```

生产环境建议使用 Redis 分布式锁 Provider。

分布式锁和幂等不是同一个概念：

| 能力 | 解决问题 |
|---|---|
| 幂等 | 同一个业务请求重复执行 |
| 分布式锁 | 多个请求同时竞争同一资源 |

两者经常一起使用。

例如创建订单：

- 幂等保证同一个请求不会重复创建订单
- 分布式锁保证同一业务资源不会被并发修改

使用分布式锁时应注意：

- 锁 Key 要有明确业务含义
- 锁粒度不要过大
- 锁超时时间不要过长
- 不要用分布式锁替代数据库唯一索引
- 不要用分布式锁包住耗时外部调用

## 分布式事务与 Outbox

微服务系统中，不同服务通常拥有独立数据库。

例如：

```text
Order Service -> Order Database
Inventory Service -> Inventory Database
Payment Service -> Payment Database
```

当一个业务流程跨越多个服务时，很难使用传统数据库事务保证所有服务同时成功或同时失败。

YSCore 推荐使用最终一致性，而不是强分布式事务。

常见方案是：

```text
本地事务
 -> Outbox 消息
 -> 消息队列
 -> 消费端处理
 -> 幂等和重试
```

Outbox 模式的核心思想是：

> 业务数据和待发送消息在同一个本地事务中保存。

这样可以避免两类常见问题：

- 数据库提交成功，但消息发送失败
- 消息发送成功，但数据库提交失败

YSCore 通过 CAP Provider 支持事务消息能力。

使用 CAP MySQL 事务处理器后，EF Core 事务可以与 CAP 消息发布协同：

```csharp
builder.Services.AddMySqlTransactionHandler();
```

之后在领域事件处理器或应用层中发布集成事件：

```csharp
await _publisher.PublishAsync(
    new OrderCreatedIntegrationEvent(order.Id, order.OrderNo),
    cancellationToken);
```

在事务提交前，CAP 会把消息写入本地消息表；事务提交后，再由 CAP 负责投递到 Kafka、RabbitMQ 等消息队列。

推荐的一致性策略：

- 单服务内部使用本地事务
- 服务之间使用集成事件
- 消息发布使用 Outbox
- 消费端必须支持幂等
- 长流程使用状态机和补偿

## 领域事件与集成事件

YSCore 明确区分领域事件和集成事件。

| 类型 | 说明 |
|---|---|
| 领域事件 | 当前服务内部发生的领域事实 |
| 集成事件 | 对其他服务发布的业务事实 |

领域事件示例：

```csharp
public record OrderPaidDomainEvent(long OrderId, string OrderNo) : IDomainEvent;
```

集成事件示例：

```csharp
public record OrderPaidIntegrationEvent(long OrderId, string OrderNo) : IIntegrationEvent;
```

推荐流程：

```text
聚合根产生领域事件
 -> UnitOfWork 分发领域事件
 -> 领域事件处理器发布集成事件
 -> CAP 投递消息
 -> 其他服务消费集成事件
```

不要直接把领域事件当成集成事件使用。

原因是：

- 领域事件属于当前服务内部模型
- 集成事件是跨服务契约
- 集成事件需要考虑版本兼容
- 集成事件不应该暴露领域实体

## 多租户

多租户用于支持 SaaS 或多组织数据隔离场景。

YSCore 多租户能力通常包括：

- 租户配置
- 租户解析
- 租户上下文
- 租户数据过滤

常见租户来源：

- Header
- Route
- Host
- Token Claims
- 当前用户上下文

领域实体可以通过实现租户过滤接口来表达租户归属：

```csharp
public class Blog : BaseEntity, IAggregateRoot, ITenantIdFilter
{
    public long TenantId { get; set; }

    public string Name { get; private set; }
}
```

EF Core Provider 可以基于租户字段进行查询过滤，避免普通业务查询跨租户读取数据。

多租户设计建议：

- 所有租户数据表都应包含 `TenantId`
- `TenantId` 应建立索引
- 后台管理端跨租户查询要显式处理
- 集成事件中如有必要应传递租户信息
- 后台任务中要显式设置租户上下文

多租户常见模式：

| 模式 | 说明 |
|---|---|
| 单库单表多租户 | 所有租户共享数据库，通过 `TenantId` 隔离 |
| 单库分表多租户 | 按租户或业务维度分表 |
| 多库多租户 | 不同租户使用不同数据库 |

具体选择取决于租户数量、数据规模、隔离要求和运维成本。

## 权限

YSCore 权限模型基于 ASP.NET Core Authorization 扩展。

框架提供：

- 用户上下文
- Claim 类型约定
- 授权特性
- 授权处理器
- Policy Provider 扩展

常见用法是在 API 或应用服务上声明权限：

```csharp
[AppAuthorize("blog.create")]
public Task<bool> Create(CreateBlogCommand command)
{
    return _mediator.Send(command);
}
```

权限编码建议使用稳定、可读的命名方式：

```text
blog.create
blog.update
blog.delete
blog.query
order.create
order.cancel
inventory.deduct
```

权限设计建议：

- API 权限和菜单权限分开管理
- 权限编码保持稳定
- 不要使用中文作为权限编码
- 多租户系统中权限应考虑租户隔离
- 数据权限应单独设计，不要只依赖接口权限

接口权限只能说明用户能不能访问某个功能。

数据权限还需要回答：

> 用户能访问哪些数据？

例如：

- 只能看自己的订单
- 只能看本部门数据
- 只能看当前租户数据
- 管理员可以跨租户查看

这些规则通常需要在 Application 层或查询层显式处理。

## 友好异常

友好异常用于表达可预期的业务错误。

例如：

- 博客不存在
- 库存不足
- 当前订单状态不允许取消
- 用户无权限操作
- 请求重复提交
- 支付回调签名无效

这些错误不是系统崩溃，而是业务规则判断后的结果。YSCore 推荐使用 `AppFriendlyException` 或 `ExceptionHelper` 来表达这类错误，而不是直接抛出普通 `Exception`。

### 开启友好异常

如果已经调用 `AddYsCore`，友好异常会随框架基础能力一起注册：

```csharp
builder.Services.AddYsCore();
```

也可以单独注册：

```csharp
using YSCore.Base.Exceptions.Extensions;

builder.Services.AddFriendlyException();
```

如果需要配置是否启用全局友好异常，可以使用：

```csharp
builder.Services.AddFriendlyException(options =>
{
    options.GlobalEnabled = true;
});
```

`GlobalEnabled` 默认为 `true`。

### 基础用法

推荐：

```csharp
throw new AppFriendlyException("博客不存在");
```

不推荐：

```csharp
throw new Exception("博客不存在");
```

带错误码：

```csharp
throw new AppFriendlyException("博客不存在", "Blog.NotFound");
```

设置 HTTP 状态码：

```csharp
using Microsoft.AspNetCore.Http;
using YSCore.Base.Exceptions.Extensions;

throw new AppFriendlyException("博客不存在", "Blog.NotFound")
    .StatusCode(StatusCodes.Status404NotFound);
```

携带额外数据：

```csharp
throw new AppFriendlyException("库存不足", "Inventory.NotEnough")
    .WithData(new
    {
        ProductId = productId,
        Required = quantity,
        Available = availableQuantity
    });
```

隐藏内部异常：

```csharp
throw new AppFriendlyException("支付处理失败", "Payment.Failed", ex)
    .WithExposeInnerException(false);
```

### 使用 ExceptionHelper

YSCore 也提供 `ExceptionHelper`，适合用错误码映射业务消息。

```csharp
using YSCore.Base.Exceptions;

throw ExceptionHelper.AppFriendly("Blog.NotFound");
```

带参数：

```csharp
throw ExceptionHelper.AppFriendly("Blog.NameExists", blogName);
```

参数会用于格式化错误消息。

校验类业务异常可以使用 `Bah`：

```csharp
throw ExceptionHelper.Bah("博客名称不能为空");
```

`Bah` 会将异常标记为验证异常，并默认使用 `400 Bad Request`。

### 配置错误码映射

可以在 `appsettings.json` 中配置友好异常：

```json
{
  "FriendlyExceptionSettings": {
    "HideErrorCode": false,
    "DefaultErrorCode": "Server.Error",
    "DefaultErrorMessage": "服务异常",
    "ThrowBah": false,
    "PropagateInnerException": false,
    "LogError": true,
    "ErrorCodeMessages": {
      "Blog.NotFound": "博客不存在",
      "Blog.NameExists": "博客名称已存在：{0}",
      "Inventory.NotEnough": "库存不足",
      "Order.CannotCancel": "当前订单状态不允许取消"
    }
  }
}
```

常用配置说明：

| 配置项 | 说明 |
|---|---|
| `HideErrorCode` | 是否在错误消息中隐藏错误码 |
| `DefaultErrorCode` | 默认错误码 |
| `DefaultErrorMessage` | 默认错误消息 |
| `ErrorCodeMessages` | 错误码和错误消息映射 |
| `ThrowBah` | 是否将 `AppFriendly` 默认视为验证异常 |
| `PropagateInnerException` | 是否对外透传内部异常 |
| `LogError` | 是否记录异常日志 |

### 业务错误码建议

错误码建议使用稳定、可读、分层的命名方式：

```text
Blog.NotFound
Blog.NameExists
Order.NotFound
Order.CannotCancel
Inventory.NotEnough
Payment.RepeatedCallback
```

不建议：

```text
001
error
系统错误
博客不存在
```

错误码是系统契约，错误消息是展示内容。错误码应该稳定，错误消息可以多语言化。

### 最佳实践

友好异常适合表达业务错误：

- 订单不存在
- 库存不足
- 当前状态不允许取消
- 用户无权限操作
- 重复提交
- 参数不符合业务规则

普通 `Exception` 适合表达系统异常：

- 数据库连接失败
- 第三方服务异常
- 程序 Bug
- 未处理的运行时异常

建议：

- 业务异常使用 `AppFriendlyException`。
- 不要把所有异常都包装成友好异常。
- 生产环境不要向客户端暴露内部异常堆栈。
- 使用稳定错误码，不要让前端依赖错误消息文本。
- 错误消息需要展示给用户时，优先走多语言资源或错误码映射。

## 统一返回值

统一返回值用于保持 API 响应格式一致。

在企业项目中，如果每个接口都返回不同结构，前端、移动端、网关和调用方都会增加大量适配成本。

YSCore 默认提供 RESTful 风格统一返回模型。

### 开启统一返回值

如果已经调用 `AddYsCore`，统一返回值会自动注册：

```csharp
builder.Services.AddYsCore();
```

也可以单独注册：

```csharp
using YSCore.Base.UnifyResult.Extensions;

builder.Services.AddUnifyResult();
```

推荐同时启用状态码拦截中间件，用于处理 401、403、404、500 等短路响应：

```csharp
using YSCore.Base.UnifyResult.Extensions;

var app = builder.Build();

app.UseUnifyResultStatusCodes();
```

中间件应放在路由和认证授权中间件的合理位置，通常可以放在认证授权之前或之后，根据项目的中间件顺序测试确认。

### 返回结构

统一返回值通常用于表达：

- 请求是否成功
- 返回数据
- 错误码
- 错误消息
- 验证错误

成功响应示例：

```json
{
  "succeeded": true,
  "data": {
    "id": 10001,
    "name": "YSCore"
  }
}
```

失败响应示例：

```json
{
  "succeeded": false,
  "error": {
    "code": "Blog.NotFound",
    "message": "博客不存在"
  }
}
```

YSCore 默认返回模型包含以下字段：

| 字段 | 说明 |
|---|---|
| `statusCode` | HTTP 状态码 |
| `succeeded` | 是否成功 |
| `data` | 成功数据 |
| `errors` | 错误信息 |
| `extras` | 附加数据 |
| `timestamp` | Unix 时间戳 |
| `traceId` | 请求追踪 ID |

### 配置统一返回值

可以在 `appsettings.json` 中配置：

```json
{
  "UnifyResultSettings": {
    "Return200StatusCodes": [401, 403],
    "SupportMvcController": false,
    "SingleValidationErrorDisplay": false
  }
}
```

常用配置说明：

| 配置项 | 说明 |
|---|---|
| `Return200StatusCodes` | 指定哪些错误状态码适配为 HTTP 200；默认包含 401、403 |
| `SupportMvcController` | 是否支持 MVC Controller 规范化处理 |
| `SingleValidationErrorDisplay` | 验证错误是否只显示第一条 |

`Return200StatusCodes` 的使用要谨慎。

如果团队希望严格遵循 HTTP 语义，可以让接口返回真实 HTTP 状态码。如果团队前端网关统一要求所有业务响应都是 200，可以配置适配规则。

### 禁用统一返回值

某些接口不应该使用统一返回值，例如：

- 文件下载
- 图片验证码
- Webhook 回调
- 第三方协议接口
- 健康检查
- 流式响应

可以使用 `[NonUnify]` 禁用：

```csharp
using YSCore.Base.UnifyResult.Attributes;

[NonUnify]
[HttpGet("download")]
public IActionResult Download()
{
    return File(bytes, "application/octet-stream", "demo.zip");
}
```

也可以标记整个 Controller：

```csharp
[NonUnify]
[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
}
```

### 自定义统一返回值 Provider

如果默认响应结构不符合团队规范，可以实现 `IUnifyResultProvider` 并注册自定义 Provider。

常见自定义场景：

- 字段名需要改为 `code`、`message`、`result`
- 成功状态需要使用业务码
- 错误结构需要兼容旧系统
- 需要额外输出 `requestId`、`tenantId`、`serverTime`

注册示例：

```csharp
builder.Services.AddUnifyResult<MyUnifyResultProvider>();
```

### 最佳实践

建议：

- 普通业务 API 使用统一返回值。
- 文件、流、第三方回调接口使用 `[NonUnify]`。
- 前端不要只依赖错误消息，应优先依赖错误码。
- 生产环境保留 `traceId`，便于排查问题。
- 不要在业务代码中手动包装统一返回值，让框架统一处理。
- 统一返回值结构一旦对外发布，应避免频繁变更。

## 数据验证

YSCore 提供数据验证能力，可以在请求进入业务处理前发现参数错误。

数据验证适合处理输入格式问题：

- 必填字段
- 字符串长度
- 数字范围
- 邮箱格式
- 手机号格式

业务规则不建议全部写成数据验证。

例如：

| 场景 | 推荐位置 |
|---|---|
| 名称不能为空 | 数据验证或领域规则 |
| 库存不能小于 0 | 领域规则 |
| 订单已支付不能取消 | 领域规则 |
| 用户无权限操作 | Application 层权限 |
| 手机号格式错误 | 数据验证 |

简单输入约束可以放在验证器中，核心业务不变量应放在领域模型中。

## 多语言

YSCore 支持基于 ASP.NET Core Localization 的多语言能力，适合国际化系统、多地区部署系统和 SaaS 系统。

多语言常见使用场景：

- 业务错误消息
- 数据验证消息
- 前端显示文本
- 枚举显示名称

### 开启多语言

注册多语言服务：

```csharp
using YSCore.Base.App.Extensions;
using YSCore.Base.Localization.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .InitYsCore();

builder.Services.AddAppLocalization();
```

启用多语言中间件：

```csharp
using YSCore.Base.Localization.Extensions;

var app = builder.Build();

app.UseAppLocalization();
```

`UseAppLocalization` 应尽量放在请求管线靠前位置，通常放在路由、认证授权和 MVC 映射之前。

### 配置多语言

在 `appsettings.json` 中添加：

```json
{
  "LocalizationSettings": {
    "ResourcesPath": "Resources",
    "SupportedCultures": [ "zh-CN", "en-US" ],
    "DefaultCulture": "zh-CN",
    "LanguageFilePrefix": "Messages",
    "AssemblyName": "YourProject.WebApi",
    "DateTimeFormatCulture": "zh-CN"
  }
}
```

配置说明：

| 配置项 | 说明 |
|---|---|
| `ResourcesPath` | 资源文件目录 |
| `SupportedCultures` | 支持的语言列表 |
| `DefaultCulture` | 默认语言 |
| `LanguageFilePrefix` | 资源文件名前缀 |
| `AssemblyName` | 资源所在程序集名称 |
| `DateTimeFormatCulture` | 固定 DateTime 格式化文化 |

### 创建资源文件

假设配置：

```json
{
  "LocalizationSettings": {
    "ResourcesPath": "Resources",
    "SupportedCultures": [ "zh-CN", "en-US" ],
    "DefaultCulture": "zh-CN"
  }
}
```

可以创建资源文件：

```text
Resources
 ├── Messages.zh-CN.resx
 └── Messages.en-US.resx
```

资源内容示例：

| Name | zh-CN Value | en-US Value |
|---|---|---|
| `Blog.NotFound` | 博客不存在 | Blog not found |
| `Blog.NameExists` | 博客名称已存在 | Blog name already exists |
| `Order.CannotCancel` | 当前订单状态不允许取消 | The current order status cannot be canceled |

### 使用多语言文本

可以通过 `L.Text` 获取本地化文本：

```csharp
using YSCore.Base.Localization;

var message = L.Text["Blog.NotFound"];
```

在异常中使用：

```csharp
throw new AppFriendlyException(L.Text["Blog.NotFound"], "Blog.NotFound");
```

也可以通过 `L.TextOf<T>()` 获取特定类型的本地化器：

```csharp
var localizer = L.TextOf<BlogService>();
var message = localizer["Blog.NotFound"];
```

### 切换当前语言

可以使用：

```csharp
L.SetCulture("en-US");
```

如果希望当前线程立即生效：

```csharp
L.SetCulture("en-US", immediately: true);
```

获取当前语言：

```csharp
var culture = L.GetSelectCulture();
```

获取支持的语言列表：

```csharp
var cultures = L.GetCultures();
```

### 请求语言来源

YSCore 使用 ASP.NET Core 的请求本地化机制，常见语言来源包括：

- QueryString
- Cookie
- `Accept-Language` Header

示例：

```http
GET /api/blogs
Accept-Language: en-US
```

也可以通过 `L.SetCulture` 将语言写入 Cookie，影响后续请求。

### 与友好异常结合

推荐使用稳定错误码，并将错误码映射到多语言资源。

例如：

```csharp
throw new AppFriendlyException(L.Text["Blog.NotFound"], "Blog.NotFound");
```

或者使用 `FriendlyExceptionSettings.ErrorCodeMessages` 配合错误码集中管理。

对于大型企业项目，推荐建立统一错误码表：

```text
Blog.NotFound
Blog.NameExists
Order.NotFound
Order.CannotCancel
Inventory.NotEnough
```

然后为每种语言维护对应资源。

### 最佳实践

建议：

- 错误码保持稳定
- 错误消息支持本地化
- 不要把业务逻辑绑定到显示文本
- API 可以根据请求语言返回对应消息
- 默认语言应明确配置
- 资源 Key 建议使用错误码或业务编码
- 不要在代码中大量硬编码用户可见文本
- 对外契约使用错误码，不使用翻译后的消息作为判断依据

例如：

```text
Blog.NotFound -> 博客不存在
Blog.NotFound -> Blog not found
```

错误码是稳定契约，错误消息是可本地化展示内容。

## 时区

企业系统经常需要处理跨地区时间问题。

YSCore 提供 `IClock`、`Clock` 和 `TimeZoneResolver`，用于统一时间来源和时区转换策略。

### 开启时钟服务

注册时钟服务：

```csharp
using YSCore.Base.TimeZone;

builder.Services.AddClock();
```

推荐配置为 UTC：

```csharp
builder.Services.AddClock(
    clock =>
    {
        clock.Kind = DateTimeKind.Utc;
    },
    timeZone =>
    {
        timeZone.DefaultTimeZoneId = "UTC";
        timeZone.TimeZoneMap["zh-CN"] = "China Standard Time";
        timeZone.TimeZoneMap["en-US"] = "Eastern Standard Time";
    });
```

`ClockOptions.Kind` 默认是 `DateTimeKind.Utc`。

### 使用 IClock

在业务代码中注入 `IClock`：

```csharp
using YSCore.Base.TimeZone;

public class CreateBlogCommandHandler : ICommandHandler<CreateBlogCommand, long>
{
    private readonly IClock _clock;

    public CreateBlogCommandHandler(IClock clock)
    {
        _clock = clock;
    }

    public async Task<long> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.Now;

        // 使用 now 设置业务时间
    }
}
```

不要在业务代码中到处直接使用：

```csharp
DateTime.Now
```

推荐统一使用：

```csharp
_clock.Now
```

这样更容易统一 UTC 策略，也更利于测试。

### Normalize 时间

`IClock.Normalize` 可以将传入时间转换到当前时钟策略：

```csharp
var normalizedTime = _clock.Normalize(inputTime);
```

当系统配置为 UTC 时，本地时间可以被转换为 UTC。

当系统配置为 Local 时，UTC 时间可以按当前解析出的时区转换为本地时间。

### 时区解析规则

`TimeZoneResolver` 的解析优先级是：

```text
1. 用户 Claims 中的 TimeZone
2. 请求头 Time-Zone
3. CultureInfo 对应的 TimeZoneMap
4. 默认时区 DefaultTimeZoneId
```

请求头示例：

```http
GET /api/orders
Time-Zone: China Standard Time
```

用户 Claims 示例：

```text
TimeZone = China Standard Time
```

如果时区 ID 无效，框架会回退到 UTC。

### TimeZoneOptions

默认时区映射包括：

```csharp
{
    { "en-US", "Eastern Standard Time" },
    { "zh-CN", "China Standard Time" },
    { "ja-JP", "Japan Standard Time" }
}
```

可以根据项目需要扩展：

```csharp
builder.Services.AddClock(
    timeZoneOptions: options =>
    {
        options.DefaultTimeZoneId = "UTC";
        options.TimeZoneMap["fr-FR"] = "Romance Standard Time";
        options.TimeZoneMap["de-DE"] = "W. Europe Standard Time";
    });
```

### 存储与展示建议

建议：

- 数据库存储优先使用 UTC 时间
- 展示时根据用户或租户时区转换
- 不要在业务代码中大量直接使用 `DateTime.Now`
- 优先使用框架提供的时钟抽象

推荐：

```csharp
CreateTime = DateTime.UtcNow;
```

更推荐：

```csharp
CreateTime = _clock.Now;
```

如果系统配置为 UTC，`_clock.Now` 会返回 UTC 时间。

### 多租户系统中的时区

SaaS 系统中，时区通常和租户或用户有关。

推荐策略：

- 数据库存储 UTC
- 用户或租户配置默认时区
- 请求中通过 Claim 或 Header 传递时区
- API 返回时间前按用户时区转换
- 后台任务明确指定时区上下文

例如：

```http
Time-Zone: China Standard Time
```

或在用户 Claims 中写入：

```text
TimeZone = China Standard Time
```

### 最佳实践

建议：

- 统一使用 UTC 存储。
- 不要混用 `DateTime.Now`、`DateTime.UtcNow` 和业务自定义时间。
- 业务代码优先依赖 `IClock`。
- 对外展示时再转换为用户时区。
- 对接第三方系统时确认对方时间格式和时区。
- 定时任务要明确使用哪个时区。
- 集成事件中的时间字段建议使用 UTC。

## 推荐启用顺序

新项目可以按以下顺序逐步启用框架能力：

```text
1. AddYsCore
2. MediatR
3. UnitOfWork
4. EF Core Provider
5. 领域事件
6. 幂等
7. 命令锁
8. CAP 集成事件
9. Redis Provider
10. 多租户
11. 权限
```

不需要在项目一开始就启用所有能力。

建议根据业务复杂度逐步引入：

- 普通单体应用：Command、UnitOfWork、Repository、FriendlyException
- 模块化单体：增加领域事件、模块契约、应用服务
- 微服务：增加集成事件、CAP、Outbox、幂等、分布式锁
- SaaS：增加多租户、数据权限、租户上下文

## 小结

YSCore 的常用能力围绕企业应用的真实问题设计。

| 能力 | 解决的问题 |
|---|---|
| 幂等 | 重复请求、重复消息 |
| 分布式锁 | 多实例并发竞争 |
| Outbox | 数据库事务与消息投递一致性 |
| 集成事件 | 服务间异步通信 |
| 多租户 | 租户识别和数据隔离 |
| 权限 | 功能访问控制 |
| 友好异常 | 业务错误表达 |
| 统一返回值 | API 响应格式一致 |
| 多语言 | 国际化错误消息 |
| 时区 | 跨地区时间一致性 |

下一章将介绍 YSCore 的 Provider 集成，包括 EF Core、CAP、Redis、Snowflake 等基础设施实现。
