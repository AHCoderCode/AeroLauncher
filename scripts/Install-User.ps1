<# Installs AeroOS only inside the current user's LocalAppData profile. No admin rights. #>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$candidateSources = @((Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts\publish'), (Join-Path $PSScriptRoot 'publish'))
$source = $candidateSources | Where-Object { Test-Path -LiteralPath $_ -PathType Container } | Select-Object -First 1
$installRoot = Join-Path $env:LOCALAPPDATA 'Programs\AeroOSWindows'
$markerName = '.aeroos-user-install'
$markerValue = 'AeroOS-Windows-UserInstall-v1'
$programs = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
$startShortcut = Join-Path $programs 'AeroOS Windows Edition.lnk'
$desktopShortcut = Join-Path ([Environment]::GetFolderPath('Desktop')) 'AeroOS Windows Edition.lnk'

if ([string]::IsNullOrWhiteSpace($source) -or -not (Test-Path -LiteralPath $source -PathType Container)) {
    throw "Build output not found. Run scripts\Build-Release.ps1 first, or extract the CI artifact and follow README.md."
}
if (Test-Path -LiteralPath $installRoot) {
    throw "AeroOS install folder already exists: $installRoot`nThis installer will not overwrite an existing folder. Use the matching AeroOS uninstaller first, or inspect the folder manually."
}
if (Test-Path -LiteralPath $startShortcut) {
    throw "A Start Menu item with the AeroOS shortcut name already exists: $startShortcut`nIt was not overwritten. Rename or inspect it before installing."
}
$desktopChoice = Read-Host 'Create a Desktop shortcut too? (Y/N)'
$makeDesktopShortcut = $desktopChoice -match '^(Y|YES)$'
if ($makeDesktopShortcut -and (Test-Path -LiteralPath $desktopShortcut)) {
    throw "A Desktop item with the AeroOS shortcut name already exists: $desktopShortcut`nIt was not overwritten. Rename or inspect it before installing."
}

$installParent = Split-Path -Parent $installRoot
if (-not (Test-Path -LiteralPath $installParent -PathType Container)) { New-Item -ItemType Directory -Path $installParent -Force | Out-Null }
New-Item -ItemType Directory -Path $installRoot | Out-Null
try {
    Copy-Item -Path (Join-Path $source '*') -Destination $installRoot -Recurse
    $exe = Join-Path $installRoot 'AeroOS.Windows.exe'
    if (-not (Test-Path -LiteralPath $exe)) { throw 'The AeroOS executable was not found in the build output.' }
    [System.IO.File]::WriteAllText((Join-Path $installRoot $markerName), $markerValue)
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall-User.ps1') -Destination (Join-Path $installRoot 'Uninstall-AeroOS.ps1')

    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($startShortcut)
    $shortcut.TargetPath = $exe
    $shortcut.WorkingDirectory = $installRoot
    $shortcut.Description = 'AeroOS Windows Edition'
    $shortcut.IconLocation = $exe
    $shortcut.Save()

    if ($makeDesktopShortcut) {
        $desktopLink = $shell.CreateShortcut($desktopShortcut)
        $desktopLink.TargetPath = $exe
        $desktopLink.WorkingDirectory = $installRoot
        $desktopLink.Description = 'AeroOS Windows Edition'
        $desktopLink.IconLocation = $exe
        $desktopLink.Save()
    }

    Write-Host ''
    Write-Host 'AeroOS Windows Edition was installed for this Windows user.' -ForegroundColor Green
    Write-Host ('Install folder: ' + $installRoot)
    Write-Host 'Windows Explorer, the Windows shell, boot files, drivers, services and personal files were not changed.'
    Write-Host 'Open it from the Start Menu. No automatic startup entry was created.'
}
catch {
    Write-Error $_
    Write-Warning "The installer may have created only the AeroOS-owned folder: $installRoot"
    Write-Warning 'Do not delete unrelated folders. Review the exact path above, then remove this AeroOS folder only if the installation did not complete and it contains no personal files.'
    throw
}
