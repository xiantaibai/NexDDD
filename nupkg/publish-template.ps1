[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Repository = "https://nuget.ourvend.com/v3/index.json",

    [Parameter(Mandatory = $false)]
    [string]$ApiKey = $env:YSCORE_NUGET_API_KEY,

    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [switch]$PackOnly
)

$ErrorActionPreference = "Stop"

$rootFolder = Split-Path -Path $PSScriptRoot -Parent
$projectPath = Join-Path $rootFolder "templates\YSCore.Ddd.Template\YSCore.Templates.Ddd.csproj"
$outputFolder = $PSScriptRoot

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Template project was not found: $projectPath"
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$projectXml = Get-Content -Raw -Encoding UTF8 -LiteralPath $projectPath
    $defaultVersion = $projectXml.Project.PropertyGroup.PackageVersion
    $inputVersion = Read-Host "Package version [$defaultVersion]"
    $Version = if ([string]::IsNullOrWhiteSpace($inputVersion)) {
        $defaultVersion
    }
    else {
        $inputVersion.Trim()
    }
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "PackageVersion was not found. Specify it with -Version."
}

$packagePath = Join-Path $outputFolder "YSCore.Templates.Ddd.$Version.nupkg"
$packArguments = @(
    "pack",
    $projectPath,
    "--configuration",
    "Release",
    "--output",
    $outputFolder,
    "-p:PackageVersion=$Version"
)

Write-Host "Packing template version $Version"
& dotnet @packArguments

if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
    throw "Template package was not found after packing: $packagePath"
}

Write-Host "Template package created: $packagePath"

if ($PackOnly) {
    Write-Host "PackOnly is enabled. NuGet push was skipped."
    return
}

$confirmation = Read-Host "Push version $Version to $Repository? (Y/N)"
if ($confirmation -notmatch "^[Yy]$") {
    Write-Host "NuGet push was canceled."
    return
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    $secureApiKey = Read-Host "NuGet API Key" -AsSecureString
    $apiKeyPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureApiKey)

    try {
        $ApiKey = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($apiKeyPointer)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($apiKeyPointer)
    }
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "NuGet API Key is missing."
}

Write-Host "Pushing template package to $Repository"
& dotnet nuget push $packagePath `
    --source $Repository `
    --api-key $ApiKey `
    --skip-duplicate

if ($LASTEXITCODE -ne 0) {
    throw "dotnet nuget push failed with exit code $LASTEXITCODE."
}

Write-Host "Template published: YSCore.Templates.Ddd@$Version"
