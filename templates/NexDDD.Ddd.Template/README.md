# NexDDD DDD 项目模板

该模板用于快速创建遵循 NexDDD 分层规范的企业应用，模板短名称为 `tpl`。

## 本地安装

在仓库根目录执行：

```powershell
dotnet new install .\templates\NexDDD.Ddd.Template
dotnet new tpl -n YourCompany.YourService
```

指定 NexDDD 包版本：

```powershell
dotnet new tpl -n YourCompany.YourService --nexddd-version 1.2.9
```

## 打包和分发

推荐从仓库根目录使用统一发布脚本：

```powershell
.\nupkg\publish-template.ps1
```

脚本会询问版本、推送确认和 NuGet API Key，其中 API Key 为隐藏输入。

仅在本地验证模板包：

```powershell
.\nupkg\publish-template.ps1 -Version 1.0.1 -PackOnly
```

也可以手工执行底层命令：

```powershell
dotnet pack .\templates\NexDDD.Ddd.Template\NexDDD.Templates.Ddd.csproj -c Release
dotnet new install .\templates\NexDDD.Ddd.Template\bin\Release\NexDDD.Templates.Ddd.1.0.0.nupkg
```

公司内部可将生成的模板包推送到 NuGet 源，开发人员随后通过包名安装：

```powershell
dotnet new install NexDDD.Templates.Ddd
dotnet new tpl -n YourCompany.YourService
```

## 生成内容

- `Application.Contracts`：Command、Query、DTO 和错误码。
- `Application`：CommandHandler、QueryHandler 和 Domain Event Handler。
- `Domain`：聚合根、Domain Event 和写仓储接口。
- `Infrastructure`：EF Core、DbContext、实体映射和仓储实现。
- `WebApi`：HTTP 接入与依赖注入组合。
- `Domain.Tests`：领域规则测试示例。

默认数据库为 MySQL 8。生成后请先修改 `appsettings.json` 中的连接字符串和雪花算法工作节点编号，再创建 EF Core Migration。
