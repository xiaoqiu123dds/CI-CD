[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Root = "."
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 显式设置控制台输出编码，避免 PowerShell 控制台乱码。
# Explicitly set console output encoding to avoid garbled PowerShell console text.
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# 受检扩展名 / Target file extensions
$extensions = @(
    ".cs",
    ".csproj",
    ".sln",
    ".config",
    ".json",
    ".xml",
    ".xaml",
    ".md",
    ".txt",
    ".ps1"
)

# 默认排除目录 / Default excluded directories
$excludedDirectories = @(
    ".git",
    ".vs",
    "bin",
    "obj",
    "packages"
)

function Test-IsExcludedPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    foreach ($directory in $excludedDirectories) {
        $pattern = "(^|[\\/])" + [Regex]::Escape($directory) + "([\\/]|$)"
        if ($Path -match $pattern) {
            return $true
        }
    }

    return $false
}

function Test-Utf8TextFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $bytes = [System.IO.File]::ReadAllBytes($Path)

    if ($bytes.Length -eq 0) {
        return [PSCustomObject]@{
            IsValid = $true
            Reason  = ""
        }
    }

    if ($bytes.Length -ge 2) {
        $bom2 = "{0:X2}{1:X2}" -f $bytes[0], $bytes[1]
        if ($bom2 -eq "FFFE" -or $bom2 -eq "FEFF") {
            return [PSCustomObject]@{
                IsValid = $false
                Reason  = "UTF-16 BOM detected"
            }
        }
    }

    if ($bytes.Length -ge 4) {
        $bom4 = "{0:X2}{1:X2}{2:X2}{3:X2}" -f $bytes[0], $bytes[1], $bytes[2], $bytes[3]
        if ($bom4 -eq "0000FEFF" -or $bom4 -eq "FFFE0000") {
            return [PSCustomObject]@{
                IsValid = $false
                Reason  = "UTF-32 BOM detected"
            }
        }
    }

    try {
        $utf8 = [System.Text.UTF8Encoding]::new($false, $true)
        $text = $utf8.GetString($bytes)
    }
    catch {
        return [PSCustomObject]@{
            IsValid = $false
            Reason  = "Not valid UTF-8 text"
        }
    }

    if ($text.IndexOf([char]0xFFFD) -ge 0) {
        return [PSCustomObject]@{
            IsValid = $false
            Reason  = "U+FFFD replacement character detected"
        }
    }

    if ($text.IndexOf([char]0x0000) -ge 0) {
        return [PSCustomObject]@{
            IsValid = $false
            Reason  = "Null character detected; possible UTF-16 or binary text"
        }
    }

    return [PSCustomObject]@{
        IsValid = $true
        Reason  = ""
    }
}

$resolvedRoot = Resolve-Path -Path $Root
$rootPath = $resolvedRoot.Path

Write-Host "Scanning root: $rootPath"

$files = Get-ChildItem -Path $rootPath -Recurse -File |
    Where-Object {
        -not (Test-IsExcludedPath -Path $_.FullName) -and $extensions -contains $_.Extension.ToLowerInvariant()
    }

$failures = New-Object System.Collections.Generic.List[object]

foreach ($file in $files) {
    $result = Test-Utf8TextFile -Path $file.FullName
    if (-not $result.IsValid) {
        $relativePath = [System.IO.Path]::GetRelativePath($rootPath, $file.FullName)
        $failures.Add([PSCustomObject]@{
            Path   = $relativePath
            Reason = $result.Reason
        }) | Out-Null
    }
}

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "Encoding check failed" -ForegroundColor Red
    foreach ($failure in $failures) {
        Write-Host (" - {0} :: {1}" -f $failure.Path, $failure.Reason) -ForegroundColor Red
    }

    exit 1
}

Write-Host ""
Write-Host ("Encoding check passed: {0} file(s) checked" -f $files.Count) -ForegroundColor Green
exit 0
