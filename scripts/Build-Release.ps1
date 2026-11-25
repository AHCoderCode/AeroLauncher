[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root 'AeroOS.sln'
$projectOutput = Join-Path $root 'src\AeroOS.Windows\bin\Release'
$artifactRoot = Join-Path $root 'artifacts'
$publish = Join-Path $artifactRoot 'publish'
$zipPath = Join-Path $artifactRoot 'AeroOS-Windows-v0.1.0.zip'

$msbuild = Get-Command 'MSBuild.exe' -ErrorAction SilentlyContinue
if ($msbuild) {
    & $msbuild.Source $solution '/m' '/p:Configuration=Release' '/p:Platform=Any CPU' '/verbosity:minimal'
    if ($LASTEXITCODE -ne 0) { throw "MSBuild failed with exit code $LASTEXITCODE" }
} else {
    $dotnet = Get-Command 'dotnet.exe' -ErrorAction SilentlyContinue
    if (-not $dotnet) { throw 'MSBuild was not found. Install Visual Studio 2022 with .NET desktop development and the .NET Framework 4.8 Developer Pack.' }
    & $dotnet.Source msbuild $solution '/m' '/p:Configuration=Release' '/p:Platform=Any CPU' '/verbosity:minimal'
    if ($LASTEXITCODE -ne 0) { throw "dotnet msbuild failed with exit code $LASTEXITCODE" }
}

if (-not (Test-Path -LiteralPath (Join-Path $projectOutput 'AeroOS.Windows.exe'))) {
    throw "Build completed but AeroOS.Windows.exe was not found at '$projectOutput'."
}
if (Test-Path -LiteralPath $publish) { throw "Publish folder already exists: $publish. Move it aside before building again; this script will not delete it." }
if (Test-Path -LiteralPath $zipPath) { throw "Release zip already exists: $zipPath. Move it aside before building again; this script will not overwrite it." }
New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
New-Item -ItemType Directory -Path $publish | Out-Null
Copy-Item -Path (Join-Path $projectOutput '*') -Destination $publish -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Install-User.ps1') -Destination $artifactRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall-User.ps1') -Destination $artifactRoot
Compress-Archive -Path (Join-Path $artifactRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "Build bundle created: $zipPath" -ForegroundColor Green
Write-Host 'The bundle is not digitally signed. Review source and run the user-level installer only after inspecting it.'

