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
Write-Host 'Portable source structure and read-only diagnostic static checks passed.' -ForegroundColor Green
Write-Host 'This static check is not a malware certification or a substitute for Windows runtime testing.'
