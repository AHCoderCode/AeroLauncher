[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$required = @(
    'AeroOS.sln',
    'VERSION',
    'README.md',
    'src\AeroOS.Windows\AeroOS.Windows.csproj',
    'src\AeroOS.Windows\App.xaml',
    'src\AeroOS.Windows\MainWindow.xaml',
    'src\AeroOS.Windows\MainWindow.xaml.cs',
    'scripts\Build-Release.ps1',
    'scripts\Get-AeroOSHardwareReport.ps1'
)
foreach ($relative in $required) {
    $path = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required project file missing: $relative" }
}

$diagnostic = Get-Content -LiteralPath (Join-Path $root 'scripts\Get-AeroOSHardwareReport.ps1') -Raw
$forbiddenPatterns = @(
    '(?im)^\s*(Set-Service|Stop-Service|Start-Service|Restart-Service|Remove-Service|Disable-ComputerRestore)\b',
    '(?im)^\s*(Format-Volume|Clear-Disk|Initialize-Disk|Set-Partition|Remove-Partition|Resize-Partition|Clear-Content|Set-Content|Add-Content|Remove-Item|Move-Item|Copy-Item)\b',
    '(?im)^\s*(bcdedit|diskpart|reg\.exe\s+add|reg\s+add|sc\.exe\s+(config|delete)|netsh\s+advfirewall\s+set)\b',
    '(?im)^\s*Set-Item(Property)?\b'
)
foreach ($pattern in $forbiddenPatterns) {
    if ([regex]::IsMatch($diagnostic, $pattern)) { throw "Safety scan failed. Diagnostic script contains a prohibited command pattern: $pattern" }
}
$appFiles = Get-ChildItem -LiteralPath (Join-Path $root 'src\AeroOS.Windows') -Recurse -File |
    Where-Object { $_.Extension -in @('.cs', '.xaml') }
$appSource = ($appFiles | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw }) -join [Environment]::NewLine
$appForbiddenPatterns = @(
    '(?i)\bRegistryKey\b|Microsoft\.Win32\.Registry',
    '(?i)\bDllImport\b|SetWindowsHookEx|CreateRemoteThread|OpenProcess',
    '(?i)\bServiceController\b|\bNew-Service\b|\bsc\.exe\b',
    '(?i)\bbcdedit\b|\bdiskpart\b|Format-Volume|Clear-Disk|Set-Partition|Remove-Partition',
    '(?i)Set-MpPreference|Add-MpPreference|DisableRealtimeMonitoring|Set-MpThreat',
    '(?i)Register-ScheduledTask|New-ScheduledTask|schtasks\.exe',
    '(?i)Environment\.SpecialFolder\.Startup',
    '(?i)Process\.(Kill|KillTree)\('
)
foreach ($pattern in $appForbiddenPatterns) {
    if ([regex]::IsMatch($appSource, $pattern)) {
        throw "Application source safety scan failed. Prohibited API or command pattern found: $pattern"
    }
}
$legacyFiles = @(
    'scripts\Install-User.ps1',
    'scripts\Uninstall-User.ps1',
    'AeroOS-Windows-source-v0.1.0.zip'
)
foreach ($relative in $legacyFiles) {
    if (Test-Path -LiteralPath (Join-Path $root $relative)) {
        throw "Legacy script-based installer or package must not be in the release source: $relative"
    }
}
Write-Host 'Application source, portable package structure and read-only diagnostic static checks passed.' -ForegroundColor Green
Write-Host 'This static check is not a malware certification or a substitute for Windows runtime testing.'
