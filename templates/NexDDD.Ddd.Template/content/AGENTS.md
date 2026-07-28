# NexDDD 项目开发规则

## 注释规则

- 所有人工编写的代码注释必须使用中文，包括 XML 文档注释、行注释、块注释、`TODO` 和配置文件注释。
- 技术名词、类型名、方法名和协议名可以保留英文，但解释这些名词的句子必须使用中文。
- 每个新增的 `class`、`interface`、`record`、`struct` 和 `enum` 都必须在类型声明上方编写中文 XML 文档注释。
- 类型注释必须说明该类型的职责、边界或协作对象，不能只重复类型名称。
- 每个 `Options` 配置类的公开属性都必须编写中文 XML 文档注释，并说明默认值、范围、单位或安全影响。

## 分层规则

- `WebApi` 只负责 HTTP 接入、认证授权、中间件和依赖注入组合。
- `Application.Contracts` 负责 Command、Query、DTO、集成事件契约和错误码。
- `Application` 负责 Handler、业务流程编排和事件处理。
- `Domain` 负责聚合根、实体、值对象、领域服务、Domain Event 和业务不变量。
- `Infrastructure` 负责 EF Core、仓储、消息、缓存和外部服务实现。
- `Domain` 不得依赖 EF Core、ASP.NET Core、CAP、Redis、HTTP Client 或 `Infrastructure`。
- 写仓储只面向聚合根；普通读取优先使用 `IQueryRepository<TEntity>`。
- 启用 UnitOfWork 管线后，CommandHandler 不得手工提交事务或调用 `SaveChangesAsync`。
