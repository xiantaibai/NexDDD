# Silky打包脚本

## 参数

| 参数名 | 是否必填 | 备注 |
|:----|:-----|:-----|
| -repo | 否 | nuget仓库地址 |
| -push | 否 | 是否将surging组件推送到nuget仓库,缺省值为`false` |
| -apikey | 否 | nuget仓库apikey,如果设置了`-push $true`,必须提供`-apikey`值 |
| -build | 否 | 是否构建surging组件包,缺省值为`true` |

## 推送nuget仓库
```
.\pack.ps1 -push $true -apikey $env:YSCORE_NUGET_API_KEY -repo "https://nuget.ourvend.com/v3/index.json"

dotnet nuget push YSCore.Provider.EntityFrameworkCore.1.0.2.nupkg `
  -s https://nuget.ourvend.com/v3/index.json `
  -k $env:YSCORE_NUGET_API_KEY `
  --skip-duplicate
```

## 发布 YSCore DDD 模板

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
