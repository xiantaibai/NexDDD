# Company.Project

本项目由 YSCore DDD 模板生成，默认包含一条完整的商品创建与查询链路。

## 项目结构

```text
src
├── Company.Project.Application.Contracts
├── Company.Project.Application
├── Company.Project.Domain
├── Company.Project.Infrastructure
└── Company.Project.WebApi
tests
└── Company.Project.Domain.Tests
```

依赖方向：

```text
WebApi -> Application + Infrastructure
Application -> Application.Contracts + Domain
Infrastructure -> Application.Contracts + Domain
Domain -> YSCore.Base
```

## 启动前准备

1. 修改 `src/Company.Project.WebApi/appsettings.json` 中的 MySQL 连接字符串。
2. 为每个服务实例分配唯一的 `Snowflake.WorkerId`。
3. 执行 `dotnet restore` 和 `dotnet build`。
4. 创建数据库 Migration，再启动 WebApi。

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project .\src\Company.Project.WebApi
```

## 示例接口

- `POST /api/products`：创建商品。
- `GET /api/products/{id}`：查询商品 DTO。

模板默认启用 UnitOfWork 管线。CommandHandler 只负责业务流程编排，不需要手工开启事务、提交事务或调用 `SaveChangesAsync`。

WebApi 通过 `AddApplicationServices` 集中注册默认能力，包括 `AddClock`、`AddCommands`、`AddServiceAndQueries`、雪花算法和基础设施。新增通用应用服务时，可实现 `ITransient`、`IScoped` 或 `ISingleton`，由程序集扫描自动注册。

模板默认使用进程内命令锁，适合本地开发和单实例部署。生产环境采用多实例部署时，应替换为 Redis 等分布式锁实现。
