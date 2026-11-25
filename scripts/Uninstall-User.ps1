<# Removes only the AeroOS user-level install folder and shortcuts created by this installer. #>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$expectedRoot = Join-Path $env:LOCALAPPDATA 'Programs\AeroOSWindows'
$installRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$expectedFullPath = [System.IO.Path]::GetFullPath($expectedRoot)
$markerPath = Join-Path $installRoot '.aeroos-user-install'
$markerValue = 'AeroOS-Windows-UserInstall-v1'

if (-not [string]::Equals($installRoot.TrimEnd('\'), $expectedFullPath.TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase)) {
    throw "Safety stop: this uninstaller only runs from '$expectedFullPath'. Current folder: '$installRoot'"
}
if (-not (Test-Path -LiteralPath $markerPath -PathType Leaf)) {
    throw 'Safety stop: AeroOS installation marker not found. No files were removed.'
}
if ((Get-Content -LiteralPath $markerPath -Raw).Trim() -ne $markerValue) {
    throw 'Safety stop: installation marker did not match. No files were removed.'
}

$answer = Read-Host 'Remove only the AeroOS program folder and shortcuts that still point to AeroOS? Preferences and all Windows files will be kept. Type REMOVE AEROOS to continue'
if ($answer -cne 'REMOVE AEROOS') { Write-Host 'Cancelled. Nothing was removed.'; exit 0 }

$app = Get-Process -Name 'AeroOS.Windows' -ErrorAction SilentlyContinue
if ($app) {
    foreach ($process in $app) {
        if ($process.MainWindowHandle -ne 0) { [void]$process.CloseMainWindow() }
    }
    Start-Sleep -Seconds 2
    if (Get-Process -Name 'AeroOS.Windows' -ErrorAction SilentlyContinue) {
        throw 'AeroOS is still running. Close its window normally, then run this uninstaller again. No files were removed.'
    }
}

$expectedExe = [System.IO.Path]::GetFullPath((Join-Path $installRoot 'AeroOS.Windows.exe'))
$startShortcut = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\AeroOS Windows Edition.lnk'
$desktopShortcut = Join-Path ([Environment]::GetFolderPath('Desktop')) 'AeroOS Windows Edition.lnk'
$shell = New-Object -ComObject WScript.Shell
foreach ($shortcutPath in @($startShortcut, $desktopShortcut)) {
    if (Test-Path -LiteralPath $shortcutPath -PathType Leaf) {
        try {
            $link = $shell.CreateShortcut($shortcutPath)
            $target = [System.IO.Path]::GetFullPath($link.TargetPath)
            if ([string]::Equals($target, $expectedExe, [StringComparison]::OrdinalIgnoreCase)) {
                Remove-Item -LiteralPath $shortcutPath -Force
            }
        } catch {
            Write-Warning "Could not verify shortcut target, so it was left untouched: $shortcutPath"
        }
    }
}

# This exact path is validated above, and the marker proves it belongs to AeroOS.
Remove-Item -LiteralPath $installRoot -Recurse -Force
Write-Host 'AeroOS program files and verified AeroOS shortcuts have been removed.' -ForegroundColor Green
Write-Host 'Windows, existing applications, drivers, services, boot files, personal documents and AeroOS preferences were left untouched.'
