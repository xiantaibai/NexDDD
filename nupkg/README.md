# NexDDD 打包与发布

## 参数

| 参数名 | 是否必填 | 备注 |
|:----|:-----|:-----|
| -repo | 否 | NuGet 仓库地址 |
| -push | 否 | 是否将 NexDDD 包推送到 NuGet 仓库，默认值为 `false` |
| -apikey | 否 | NuGet API Key；设置 `-push $true` 时必须提供 |
| -build | 否 | 是否构建 NexDDD 包，默认值为 `true` |

## 推送 NuGet 仓库
```
.\pack.ps1 -push $true -apikey $env:NEXDDD_NUGET_API_KEY -repo "https://nuget.ourvend.com/v3/index.json"

dotnet nuget push NexDDD.Provider.EntityFrameworkCore.1.0.2.nupkg `
  -s https://nuget.ourvend.com/v3/index.json `
  -k $env:NEXDDD_NUGET_API_KEY `
  --skip-duplicate
```

## 发布 NexDDD DDD 模板

在仓库根目录直接运行：

```powershell
.\nupkg\publish-template.ps1
```

脚本会依次询问发布版本、是否推送和 NuGet API Key。API Key 使用隐藏输入，不会显示在控制台。

在 `nupkg` 目录中运行：

```powershell
.\publish-template.ps1
```

也可以提前指定版本：

```powershell
.\nupkg\publish-template.ps1 -Version 1.0.1
```

仅打包、不推送：

```powershell
.\nupkg\publish-template.ps1 -Version 1.0.1 -PackOnly
```

脚本默认推送到 `https://nuget.ourvend.com/v3/index.json`，并启用 `--skip-duplicate`。
