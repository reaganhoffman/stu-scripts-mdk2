<#
.SYNOPSIS
    Builds (and, for Release, deploys) the MDK2 script project that owns a given file.

.DESCRIPTION
    Walks up the directory tree from -FromPath to find the nearest *.csproj, then
    runs `dotnet build`. MDK2's PbPackager runs as part of the build: a Release
    build minifies the script and writes it to the Space Engineers local scripts
    folder (output=auto -> %APPDATA%\SpaceEngineers\IngameScripts\local\<Project>),
    so it shows up in-game under "Browse Scripts".

.PARAMETER FromPath
    Any file or folder inside the script project (typically the active editor file).

.PARAMETER Configuration
    Release (default, deploys) or Debug.
#>
param(
    [Parameter(Mandatory = $true)][string]$FromPath,
    [ValidateSet("Release", "Debug")][string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Resolve the starting directory.
if (Test-Path -LiteralPath $FromPath -PathType Container) {
    $dir = Get-Item -LiteralPath $FromPath
}
else {
    $dir = (Get-Item -LiteralPath $FromPath).Directory
}

# Walk up to the nearest project file.
$proj = $null
while ($null -ne $dir) {
    $candidate = Get-ChildItem -LiteralPath $dir.FullName -Filter *.csproj -File -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($candidate) { $proj = $candidate; break }
    $dir = $dir.Parent
}

if (-not $proj) {
    Write-Error "No .csproj found at or above '$FromPath'. Open a file inside a script project and try again."
    exit 1
}

Write-Host "==> $Configuration build: $($proj.FullName)" -ForegroundColor Cyan
dotnet build $proj.FullName -c $Configuration
$code = $LASTEXITCODE
if ($code -eq 0 -and $Configuration -eq "Release") {
    Write-Host "==> Deployed. Find it in-game via Programmable Block -> Edit -> Browse Scripts -> '$($proj.BaseName)'." -ForegroundColor Green
}
exit $code
