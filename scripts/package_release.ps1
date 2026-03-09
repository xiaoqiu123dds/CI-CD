[CmdletBinding()]
param(
    [string]$ProjectPath = "src/PrismReactiveDemo/PrismReactiveDemo.csproj",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "dev-local",
    [bool]$SelfContained = $false,
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$resolvedProjectPath = (Resolve-Path (Join-Path $repositoryRoot $ProjectPath)).Path
$publishRoot = Join-Path $repositoryRoot "artifacts\publish\$Runtime\$Version"
$releaseRoot = Join-Path $repositoryRoot 'artifacts\releases'
$bundleName = "PrismReactiveDemo-$Version-$Runtime"
$bundlePath = Join-Path $releaseRoot "$bundleName.zip"
$checksumPath = Join-Path $releaseRoot "$bundleName.sha256"

New-Item -ItemType Directory -Path $publishRoot -Force | Out-Null
New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null

if (Test-Path $bundlePath)
{
    Remove-Item $bundlePath -Force
}

if (Test-Path $checksumPath)
{
    Remove-Item $checksumPath -Force
}

Get-ChildItem $publishRoot -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force

if (-not $SkipRestore)
{
    & dotnet restore $resolvedProjectPath
    if ($LASTEXITCODE -ne 0)
    {
        throw "dotnet restore failed."
    }
}

$publishArgs = @(
    'publish'
    $resolvedProjectPath
    '-c'
    $Configuration
    '-r'
    $Runtime
    '--self-contained'
    $SelfContained.ToString().ToLowerInvariant()
    '-p:DebugType=None'
    '-p:DebugSymbols=false'
    '-o'
    $publishRoot
)

& dotnet @publishArgs
if ($LASTEXITCODE -ne 0)
{
    throw "dotnet publish failed."
}

Compress-Archive -Path (Join-Path $publishRoot '*') -DestinationPath $bundlePath -Force

$hash = (Get-FileHash -Path $bundlePath -Algorithm SHA256).Hash.ToLowerInvariant()
@(
    "File: $(Split-Path $bundlePath -Leaf)"
    "SHA256: $hash"
) | Set-Content -Path $checksumPath -Encoding utf8

Write-Host "Bundle created: $bundlePath"
Write-Host "Checksum file: $checksumPath"
