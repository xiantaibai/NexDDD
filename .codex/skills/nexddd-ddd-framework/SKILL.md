---
name: nexddd-ddd-framework
description: 指导 Codex 在 NexDDD DDD framework 项目中实现业务功能。Use when adding or modifying NexDDD/.NET features involving DDD layers, Command, CommandHandler, Query, DTO, Repository, UnitOfWork, Domain Event, Integration Event, Idempotency, Command Lock, EF Core, CAP, Redis, or WebApi endpoint.
---

# NexDDD DDD Framework

当用户要在 NexDDD 框架中新增或修改业务功能时，使用这个 skill。目标不是泛泛讲 DDD，而是让 AI 按这个项目的架构思想写出能落地、能维护的代码。

整体表达风格：

- 规则说明优先用中文。
- 技术关键词保留英文，例如 `Domain`、`Application`、`CommandHandler`、`UnitOfWork`。
- 代码示例使用英文类名、方法名、属性名。
- 架构判断用中文说明“为什么这样放、为什么不能那样写”。

## 先阅读

开始改代码前，先观察当前项目结构、命名方式和已有示例。项目已有约定优先于通用模板。

需要代码模板、注册方式或完整检查清单时，读取 [references/implementation-guide.md](references/implementation-guide.md)。

## 架构思想

NexDDD 推荐的核心思想是：业务规则收敛到 `Domain`，业务流程编排放在 `Application`，对外契约放在 `Application.Contracts`，数据持久化与技术能力落地放在 `Infrastructure`，入口层 `WebApi` 保持轻薄。

不要把系统写成“Controller + DbContext”的事务脚本。AI 实现功能时，要先判断业务概念属于哪个层，再决定写在哪个项目里。

## 分层规则

`WebApi` 只负责接入：

- Controller / Minimal API endpoint
- Authentication / Authorization
- Swagger / Middleware / CORS
- DI composition
- 通过 `IMediator.Send(...)` 转发 `Command` 或 `Query`

`Application.Contracts` 负责稳定契约：

- `Command`
- `Query`
- `DTO`
- 应用服务接口
- `IntegrationEvent` contract
- error code 常量

`Application` 负责业务流程编排：

- `CommandHandler`
- `QueryHandler`
- Domain Event Handler
- Integration Event Handler
- `IIdempotentCommand<TCommand>`
- `ICommandLock<TCommand>`
- 应用层权限检查和流程编排

`Application` 关注“一个业务用例应该按什么顺序发生”：加载聚合、调用领域行为、协调 Repository、发布应用侧副作用、返回结果。它不负责数据库表结构、EF mapping、Redis/CAP provider 细节，也不承载核心领域不变量。

`Domain` 负责业务本质：

- Aggregate Root
- Entity
- Value Object
- Domain Service
- Domain Event
- Enumeration
- 业务不变量和领域异常

`Infrastructure` 负责数据持久化的落地和技术能力实现：

- EF Core `DbContext`
- `IEntityTypeConfiguration<T>`
- Repository implementation
- CAP / Redis / third-party provider
- Migration
- 外部服务实现

`Infrastructure` 关注“业务对象如何被保存、查询、投递或连接外部系统”：它实现 `Application` 或 `Domain` 需要的技术接口，但不编排业务流程，不决定业务规则。

## 依赖方向

必须遵守依赖方向：

- `Domain` 只依赖 `NexDDD.Core` 和领域内部代码。
- `Application` 可以依赖 `Application.Contracts` 和 `Domain`。
- `Infrastructure` 可以依赖 `Domain`，必要时依赖 `Application.Contracts`。
- `WebApi` 组合 `Application`、`Application.Contracts`、`Infrastructure`。

禁止让 `Domain` 依赖 `EF Core`、`ASP.NET Core`、`CAP`、`Redis`、HTTP Client、第三方 SDK 或 `Infrastructure`。

## 写操作流程

实现一个写用例时，按这个顺序思考：

1. 在 `Application.Contracts` 定义 `Command`：`VerbNounCommand : ICommand<TResult>`。
2. 在 `Domain` 修改或创建 Aggregate Root，把业务规则写进聚合方法。
3. 需要表达“领域事实已经发生”时，在聚合中 `AddDomainEvent(...)`。
4. 在 `Application` 实现一个 `ICommandHandler<TCommand, TResult>`。
5. 需要本地副作用或发布跨服务消息时，在 `Application` 写 Domain Event Handler。
6. 写仓储 `INexRepository<TEntity>` 面向 Aggregate Root 设计；接口表达领域写入需要，具体数据持久化落地在 `Infrastructure`。
7. EF Core mapping 放在 `Infrastructure`。
8. `WebApi` 只暴露轻量 endpoint/controller，并调用 `IMediator.Send(...)`。
9. 风险较高的规则要补 `Domain` 或 `Application` 测试。

`CommandHandler` 的职责是编排用例，不是承载所有业务规则。典型流程是：加载聚合、调用聚合行为、保存聚合、返回结果。

## 读写仓储规则

NexDDD 仓储分成两类：

- 写仓储：`INexRepository<TEntity>` / `INexRepository<TEntity, TKey>`，只面向 Aggregate Root，用于聚合创建、修改、删除和领域写入。
- 读仓储：`IQueryRepository<TEntity>` / `IReadableRepository<TEntity>`，可以面向任意 `Entity`，用于查询、投影、只读库、从库或组合读取。

AI 实现功能时要先判断读写意图：

- 写用例优先注入聚合专属 Repository，例如 `IOrderRepository : INexRepository<Order, long>`。
- 普通读用例优先注入通用 `IQueryRepository<TEntity>`，不要为了简单查询额外声明一堆读仓储接口。
- 只有默认 `IQueryRepository<TEntity>` 不满足时，才定义自定义 Query Repository，例如需要复杂 SQL、跨数据源组合、Redis/local cache 读取、读模型性能优化。
- 自定义 Query Repository 应放在 `Infrastructure` 实现，接口可以放在 `Application.Contracts` 或 `Application` 可访问的位置，取决于业务项目的依赖约定。
- 查询仓储可以混用 database、Redis、本地存储或第三方查询能力，但它表达的是“读模型获取方式”，不要把业务流程编排塞进仓储。
- `Application` 负责业务流程编排，可以使用读仓储接口；`Infrastructure` 负责把读仓储落地到 EF Core、SQL、Redis 或其他 provider。

默认注册规则：

- 系统注册时应默认支持 `IQueryRepository<TEntity>` 和 `IReadableRepository<TEntity>` 的通用实现。
- 使用 `AddQueryRepositories<TDbContext>(entityAssemblies)` 注册实体程序集中的默认查询仓储。
- 使用 `AddReadableRepositories<TDbContext>(assemblies)` 同时注册默认查询仓储和自定义只读/查询仓储。
- 使用 `AddWriteRepositories(assemblies)` 注册聚合写仓储；旧的 `AddRepositories(...)` 保留兼容，内部等价于写仓储注册。

## 读操作流程

实现一个读用例时：

1. 在 `Application.Contracts` 定义 `Query` 和 `DTO`。
2. 在 `Application` 实现 `IRequestHandler<TQuery, TDto>`,或者 IQuery 接口。
3. 普通查询优先注入 `IQueryRepository<TEntity>`。
4. 查询使用 `AsNoTracking()`。
5. 使用 projection 直接投影到 DTO。
6. 不要返回 Domain Entity 或 EF Entity。

复杂读模型可以通过自定义 Query Repository 直接使用 `DbContext`、raw SQL、Redis 或其他存储做投影查询，不必强行加载完整聚合。

## UnitOfWork 思想

NexDDD 的 `Command` 管线负责事务和保存：

- `cfg.AddUnitOfWorkBehaviors()` 注册 UnitOfWork pipeline。
- `services.AddUnitOfWork<TDbContext>()` 注册 DbContext 工作单元。
- `CommandUnitOfWorkBehavior` 会开启 transaction、执行 handler、调用 `SaveEntitiesAsync`、标记 idempotency completed、commit。
- `AppDbContextBase.SaveEntitiesAsync` 会保存 EF 变更并 dispatch Domain Events。

因此，启用 UnitOfWork pipeline 后，`CommandHandler` 通常不要：

- 手动 `SaveChangesAsync`
- 手动开启 transaction
- 手动 commit / rollback
- 手动 dispatch Domain Events

## Domain Event 和 Integration Event

`Domain Event` 表达当前服务内部已经发生的领域事实，例如：

```csharp
public record OrderPaidDomainEvent(long OrderId, string OrderNo) : IDomainEvent;
```

`Integration Event` 是跨服务契约，例如：

```csharp
public record OrderPaidIntegrationEvent(long OrderId, string OrderNo) : IIntegrationEvent;
```

推荐流程：

```text
Aggregate Root raises Domain Event
 -> UnitOfWork dispatches Domain Event
 -> Domain Event Handler publishes Integration Event
 -> CAP / MQ delivers message
 -> Other service handles Integration Event
```

不要直接把 `Domain Event` 当成 `Integration Event` 用。`Integration Event` 不能暴露聚合对象或领域实体，只传稳定字段。

## 幂等和命令锁

重复请求使用 `Idempotency`：同一个业务请求重复提交，只执行一次业务结果。

并发竞争使用 `Command Lock`：多个请求同时操作同一业务资源时，串行化关键资源。

两者解决的问题不同，经常一起使用：

- `Idempotency` 处理重复提交。
- `Command Lock` 处理并发竞争。

注册 pipeline 时，优先保持项目已有顺序。没有既有约定时，通常按 `Command Lock`、`Idempotency`、`UnitOfWork` 的顺序注册。

`Idempotency` 是可选能力，不是所有 `Command` 的必选依赖：

- 只使用 `Command` + `UnitOfWork` 时，不需要注册幂等服务，也不应该因为缺少幂等上下文报错。
- 显式启用 `AddIdempotentBehavior()` 时，必须同时注册 `AddIdempotency(...)` 或 `AddDbIdempotency<TDbContext>(...)`，并提供 `IIdempotencyStore`。
- 只有某个命令存在 `IIdempotentCommand<TCommand>` 配置时，才表示该命令需要幂等保护。
- `CommandUnitOfWorkBehavior` 可以感知幂等上下文，但不能强制要求业务系统注册幂等上下文。

## 禁止事项

- 不要把业务规则写在 Controller 或 Minimal API lambda 中。
- 不要从 API 或 Query 返回 Domain Entity。
- 不要让 `Command` 包含 `ToEntity()` 或业务逻辑。
- 不要绕过 Aggregate Root 修改子实体。
- 不要在 `Domain` 里调用 CAP、Redis、HTTP Client 或第三方 SDK。
- 不要把领域实体放进 `IntegrationEvent` payload。
- 不要在启用 UnitOfWork pipeline 后，在 `CommandHandler` 里手写 transaction。
- 不要为了读列表强行加载完整聚合。
- 不要为了普通查询过度声明专属读仓储；默认 `IQueryRepository<TEntity>` 能满足时直接使用默认实现。
- 不要让写仓储承担 Redis/local cache/跨库读模型拼装；这些属于 Query Repository 或独立查询服务。
