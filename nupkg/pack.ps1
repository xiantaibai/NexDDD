Param(
  [parameter(Mandatory=$false)][string]$repo="http://nuget.ourvend.com/v3/index.json",
  [parameter(Mandatory=$false)][bool]$push=$true,
  [parameter(Mandatory=$false)][string]$apikey='Aa121121',
  [parameter(Mandatory=$false)][bool]$build=$true
)

# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$rootFolder = Join-Path $packFolder ".."
## $srcPath = Join-Path $packFolder "../"
$srcPath= Split-Path -Path $packFolder -Parent

Write-Output "packFolder: $packFolder"
Write-Output "rootFolder: $rootFolder"
Write-Output "srcPath: $srcPath"

$projects = (Get-Content "./Components")

Write-Output "projects: $projects"

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

function Pack($projectFolder,$projectName) {  
  Set-Location $projectFolder
  $releaseProjectFolder = (Join-Path $projectFolder "bin/Release")
  if (Test-Path $releaseProjectFolder)
  {
     Remove-Item -Force -Recurse $releaseProjectFolder
  }
  
   & dotnet restore
   & dotnet pack -c Release
   if ($projectName) {
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $projectName + ".*.nupkg")
   }else {
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $project + ".*.nupkg")
   }
   Move-Item -Force $projectPackPath $packFolder 
}

if (Ask-Continue -Message "Proceed with the build? (Y/N)") {

    if ($build) {
      foreach($project in $projects) {
        if (-not $project.StartsWith("#")){
          $projectName = ($project -Split "/" )[-1]
          $projectFolder = Join-Path $srcPath $project
          Write-Output "projectFolder: $projectFolder"
          Pack -projectFolder $projectFolder -projectName $projectName 
        }    
      }
      Set-Location $packFolder
    }

    if($push) {
        $apikey="$Aa121121"
        if ([string]::IsNullOrEmpty($apikey)){
            Write-Warning -Message "未设置nuget仓库的APIKEY"
		    exit 1
	    }

	    foreach($project in $projects) {
           $projectFolder = Join-Path $srcPath $project
    	    [xml]$propsXml = Get-Content (Join-Path $projectFolder "Directory.Build.props")
          $version = $propsXml.Project.PropertyGroup.VersionPrefix
          $projectName = ($project -Split "/" )[-1]
          & dotnet nuget push ($projectName + "." + $version + ".nupkg") -s $repo -k $apikey --skip-duplicate
        }
    }
}
else {
    Write-Output "Build step skipped."
}


 foreach ($projectFile in $projectFiles) {
    Write-Output "Processing project: $($projectFile.FullName)"
    
    # 进入项目文件所在目录
    Push-Location -Path $projectFile.DirectoryName
    
    # 生成项目
    ## dotnet build $projectFile.Name -c Release
    
    # 创建 NuGet 包
    ## dotnet pack $projectFile.Name -c Release -o .

    # 查找生成的 .nupkg 文件
     $nupkgFiles = Get-ChildItem -Path $projectFile.DirectoryName -Filter *.nupkg
    foreach ($nupkgFile in $nupkgFiles) {
        Move-Item -Force $nupkgFile $packFolder 
        Write-Output "Publishing package: $($nupkgFile.FullName)"
        $fileName = Split-Path -Path $filePath -Leaf
        Write-Output "Publishing package: $($fileName)"
        
        # 发布到 NuGet
        ## nuget push $nupkgFile.FullName -ApiKey $nugetApiKey -Source $nugetSource
        ## dotnet nuget push ($nupkgFile.FullName) -s $repo -k $apikey --skip-duplicate
    }

    # 返回原始路径
    Pop-Location
}