Param(
  [parameter(Mandatory=$false)][string]$repo="http://nuget.ourvend.com/v3/index.json",
  [parameter(Mandatory=$false)][bool]$push=$true,
  [parameter(Mandatory=$false)][string]$apikey='Aa121121',
  [parameter(Mandatory=$false)][bool]$build=$true
)

# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$rootFolder = Join-Path $packFolder ".."
$srcPath = Join-Path $packFolder "../"



$projects = (Get-Content "./Components")

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

if ($build) {
  foreach($project in $projects) {
    if (-not $project.StartsWith("#")){
      $projectName = ($project -Split "/" )[-1]
      $projectFolder = Join-Path $srcPath $project
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
      $version = $propsXml.Project.PropertyGroup.Version
      $projectName = ($project -Split "/" )[-1]
      & dotnet nuget push ($projectName + "." + $version + ".nupkg") -s $repo -k $apikey --skip-duplicate
    }
}