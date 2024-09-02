Param(
  [parameter(Mandatory=$false)][string]$repo="http://nuget.ourvend.com/v3/index.json",
  [parameter(Mandatory=$false)][bool]$push=$true,
  [parameter(Mandatory=$false)][string]$apikey='Aa121121',
  [parameter(Mandatory=$false)][bool]$build=$true
)

# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
## $rootFolder = Join-Path $packFolder ".."
## $srcPath = Join-Path $packFolder "../"
$rootFolder = Split-Path -Path $packFolder -Parent
$srcPath= Join-Path $rootFolder "/src"

Write-Output "packFolder: $packFolder"
Write-Output "rootFolder: $rootFolder"
Write-Output "srcPath: $srcPath"

## $projects = (Get-Content "./Components")

# 获取所有 .csproj 文件
$projectFiles = Get-ChildItem -Path $srcPath -Recurse -Filter *.csproj

Write-Output "projects: $projectFiles"


# 询问用户是否继续
function Ask-Continue {
    param (
        [string]$Message = "Do you want to continue? (Y/N)"
    )
    $response = Read-Host $Message
    switch ($response.ToUpper()) {
        'Y' {
            return $true
        }
        'N' {
            Write-Output "Operation canceled."
            return $false
        }
        default {
            Write-Output "Invalid input. Please enter 'Y' or 'N'."
            return (Ask-Continue -Message $Message)
        }
    }
}

if (Ask-Continue -Message "只推送Nuget? (Y/N)"){
    $nupkgFiles=  Get-ChildItem -Path $packFolder -Recurse -Filter *.nupkg
    foreach ($nupkgFile in $nupkgFiles) {
       ##  Move-Item -Force $nupkgFile $packFolder 
        Write-Output "Publishing package: $($nupkgFile.FullName)"
        $fileName = Split-Path -Path $nupkgFile -Leaf
        Write-Output "filename: $($fileName)"
        
        # 发布到 NuGet
        dotnet nuget push ($nupkgFile.FullName) -s $repo -k $apikey --skip-duplicate
    }
}else{
    foreach ($projectFile in $projectFiles) {
        Write-Output "Processing project: $($projectFile.FullName)"
    
        # 进入项目文件所在目录
        Push-Location -Path $projectFile.DirectoryName
    
        # 生成项目
        dotnet build $projectFile.Name -c Release
    
        # 创建 NuGet 包
        dotnet pack $projectFile.Name -c Release -o .

        # 查找生成的 .nupkg 文件
        $nupkgFiles = Get-ChildItem -Path $projectFile.DirectoryName -Filter *.nupkg
        foreach ($nupkgFile in $nupkgFiles) {
            Move-Item -Force $nupkgFile $packFolder 
            Write-Output "Publishing package: $($nupkgFile.FullName)"
            $fileName = Split-Path -Path $nupkgFile -Leaf
            Write-Output "filename: $($fileName)"
        
             # 发布到 NuGet
             ## dotnet nuget push ($fileName) -s $repo -k $apikey --skip-duplicate
        }

        # 返回原始路径
        Pop-Location
    }
}

if (Ask-Continue -Message "Proceed with the build? (Y/N)"){
}