[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root 'AeroOS.sln'
$projectOutput = Join-Path $root 'src\AeroOS.Windows\bin\Release'
$artifactRoot = Join-Path $root 'artifacts'
$publish = Join-Path $artifactRoot 'portable'
$version = (Get-Content -LiteralPath (Join-Path $root 'VERSION') -Raw).Trim()
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "Invalid VERSION value: $version" }
$bundleName = "AeroOS-Windows-v$version"
$zipPath = Join-Path $artifactRoot ($bundleName + '.zip')
$hashPath = $zipPath + '.sha256'

$msbuild = Get-Command 'MSBuild.exe' -ErrorAction SilentlyContinue
if ($msbuild) {
    & $msbuild.Source $solution '/m' '/p:Configuration=Release' '/p:Platform=Any CPU' '/verbosity:minimal'
    if ($LASTEXITCODE -ne 0) { throw "MSBuild failed with exit code $LASTEXITCODE" }
} else {
    $dotnet = Get-Command 'dotnet.exe' -ErrorAction SilentlyContinue
    if (-not $dotnet) { throw 'MSBuild was not found. Use a Windows build runner with Visual Studio Build Tools and the .NET Framework 4.8 Developer Pack.' }
    & $dotnet.Source msbuild $solution '/m' '/p:Configuration=Release' '/p:Platform=Any CPU' '/verbosity:minimal'
    if ($LASTEXITCODE -ne 0) { throw "dotnet msbuild failed with exit code $LASTEXITCODE" }
}

$exe = Join-Path $projectOutput 'AeroOS.Windows.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw "Build completed but AeroOS.Windows.exe was not found at '$projectOutput'." }
if (Test-Path -LiteralPath $publish) { throw "Portable staging folder already exists: $publish. This script will not delete or overwrite it." }
if (Test-Path -LiteralPath $zipPath) { throw "Release bundle already exists: $zipPath. This script will not overwrite it." }

New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
New-Item -ItemType Directory -Path $publish | Out-Null
Get-ChildItem -LiteralPath $projectOutput -File | Where-Object { $_.Extension -ne '.pdb' } | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $publish
}
Copy-Item -LiteralPath (Join-Path $root 'README.md') -Destination (Join-Path $publish 'README-AeroOS.txt')
$notes = @'
AeroOS Windows Edition is a portable desktop launcher window.
Extract this folder and run AeroOS.Windows.exe.
There is no installer, service, scheduled task, automatic startup entry, shell replacement or boot modification.
Close AeroOS with the Exit AeroOS button or Alt+F4 to return to the normal Windows desktop.
AeroOS launches existing Windows applications; it does not install or modify them.
The app is unsigned. Review the source and scan the downloaded bundle before running it.
'@
Set-Content -LiteralPath (Join-Path $publish 'AEROOS-INFO.txt') -Value $notes -Encoding UTF8
Compress-Archive -Path (Join-Path $publish '*') -DestinationPath $zipPath -CompressionLevel Optimal
$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath $hashPath -Value ($hash + '  ' + [IO.Path]::GetFileName($zipPath)) -Encoding ASCII
Write-Host "Portable AeroOS bundle created: $zipPath" -ForegroundColor Green
Write-Host "SHA-256: $hash"
Write-Host 'The bundle contains the application only; no installer or uninstall scripts are packaged.'
