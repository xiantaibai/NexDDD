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

$nupkgFiles=  Get-ChildItem -Path $packFolder -Recurse -Filter *.nupkg
foreach ($nupkgFile in $nupkgFiles) {
       ##  Move-Item -Force $nupkgFile $packFolder 
        Write-Output "Publishing package: $($nupkgFile.FullName)"
        $fileName = Split-Path -Path $nupkgFile -Leaf
        Write-Output "filename: $($fileName)"
        
        # 发布到 NuGet
        ## nuget push $nupkgFile.FullName -ApiKey $nugetApiKey -Source $nugetSource
        dotnet nuget push ($nupkgFile.FullName) -s $repo -k $apikey --skip-duplicate
    }

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


# 遍历每个项目文件并发布

if (Ask-Continue -Message "Proceed with the build? (Y/N)"){

}