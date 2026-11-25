<#
.SYNOPSIS
  Collects a read-only Windows hardware and compatibility report for AeroOS.
.DESCRIPTION
  Reads Windows/WMI/PowerShell inventory only. It does not change services,
  registry values, drivers, startup configuration, power settings, partitions,
  boot files, or Windows installation state. By default it prints to the console.
.PARAMETER OutputPath
  Optional path for a new report file. Existing files are never overwritten.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Continue'
$lines = New-Object System.Collections.Generic.List[string]
function Add-ReportLine {
    param([string]$Value = '')
    [void]$script:lines.Add($Value)
}
function Add-Section {
    param([string]$Title)
    Add-ReportLine ''
    Add-ReportLine ('=' * 72)
    Add-ReportLine $Title
    Add-ReportLine ('=' * 72)
}
function Format-GB {
    param([object]$Bytes)
    if ($null -eq $Bytes) { return 'Unknown' }
    try { return ('{0:N1} GB' -f ([double]$Bytes / 1GB)) } catch { return 'Unknown' }
}

Add-ReportLine 'AEROOS WINDOWS EDITION - READ-ONLY HARDWARE REPORT'
Add-ReportLine ('Generated: ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz'))
Add-ReportLine 'Collection mode: read-only system inventory'
Add-ReportLine 'Personal identifiers, serial numbers, IP addresses and command-line arguments are intentionally omitted.'

Add-Section 'OPERATING SYSTEM'
try {
    $os = Get-CimInstance -ClassName Win32_OperatingSystem -ErrorAction Stop
    Add-ReportLine ('Caption: ' + $os.Caption)
    Add-ReportLine ('Version: ' + $os.Version)
    Add-ReportLine ('Build: ' + $os.BuildNumber)
    Add-ReportLine ('Architecture: ' + $os.OSArchitecture)
    Add-ReportLine ('Last boot: ' + $os.LastBootUpTime)
    Add-ReportLine ('Windows directory: ' + $os.WindowsDirectory)
    Add-ReportLine ('Total physical memory: ' + (Format-GB ([double]$os.TotalVisibleMemorySize * 1KB)))
    Add-ReportLine ('Available physical memory: ' + (Format-GB ([double]$os.FreePhysicalMemory * 1KB)))
} catch { Add-ReportLine ('OS query unavailable: ' + $_.Exception.Message) }

Add-Section 'COMPUTER MODEL AND FIRMWARE'
try {
    $computer = Get-CimInstance -ClassName Win32_ComputerSystem -ErrorAction Stop
    Add-ReportLine ('Manufacturer: ' + $computer.Manufacturer)
    Add-ReportLine ('Model: ' + $computer.Model)
    Add-ReportLine ('Physical cores: ' + $computer.NumberOfProcessors)
    Add-ReportLine ('Logical processors: ' + $computer.NumberOfLogicalProcessors)
} catch { Add-ReportLine ('Computer model query unavailable: ' + $_.Exception.Message) }
try {
    $bios = Get-CimInstance -ClassName Win32_BIOS -ErrorAction Stop
    Add-ReportLine ('BIOS manufacturer: ' + $bios.Manufacturer)
    Add-ReportLine ('BIOS version: ' + (($bios.SMBIOSBIOSVersion -join ', ')))
    Add-ReportLine ('BIOS release date: ' + $bios.ReleaseDate)
} catch { Add-ReportLine ('BIOS query unavailable: ' + $_.Exception.Message) }
try {
    $secureBoot = Confirm-SecureBootUEFI -ErrorAction Stop
    Add-ReportLine ('Secure Boot: ' + [string]$secureBoot)
} catch { Add-ReportLine 'Secure Boot: unavailable (may be Legacy BIOS, unsupported firmware, or insufficient access)' }
try {
    $tpm = Get-Tpm -ErrorAction Stop
    Add-ReportLine ('TPM present: ' + [string]$tpm.TpmPresent)
    Add-ReportLine ('TPM ready: ' + [string]$tpm.TpmReady)
} catch { Add-ReportLine 'TPM information: unavailable on this Windows build or without the relevant module/access' }

Add-Section 'PROCESSOR'
try {
    Get-CimInstance -ClassName Win32_Processor -ErrorAction Stop | ForEach-Object {
        Add-ReportLine ('Name: ' + $_.Name)
        Add-ReportLine ('Manufacturer: ' + $_.Manufacturer)
        Add-ReportLine ('Cores: ' + $_.NumberOfCores)
        Add-ReportLine ('Logical processors: ' + $_.NumberOfLogicalProcessors)
        Add-ReportLine ('Max clock MHz: ' + $_.MaxClockSpeed)
        Add-ReportLine ('Current clock MHz: ' + $_.CurrentClockSpeed)
        Add-ReportLine ('Reported CPU load: ' + $_.LoadPercentage + '%')
    }
} catch { Add-ReportLine ('CPU query unavailable: ' + $_.Exception.Message) }

Add-Section 'GRAPHICS'
try {
    Get-CimInstance -ClassName Win32_VideoController -ErrorAction Stop | ForEach-Object {
        Add-ReportLine ('GPU: ' + $_.Name)
        Add-ReportLine ('Driver version: ' + $_.DriverVersion)
        Add-ReportLine ('Video mode: ' + $_.VideoModeDescription)
        Add-ReportLine ('Adapter RAM reported: ' + (Format-GB $_.AdapterRAM))
    }
} catch { Add-ReportLine ('GPU query unavailable: ' + $_.Exception.Message) }

Add-Section 'PHYSICAL DISKS'
try {
    Get-CimInstance -ClassName Win32_DiskDrive -ErrorAction Stop | ForEach-Object {
        Add-ReportLine ('Model: ' + $_.Model)
        Add-ReportLine ('Interface: ' + $_.InterfaceType)
        Add-ReportLine ('Capacity: ' + (Format-GB $_.Size))
        Add-ReportLine ('Media descriptor: ' + $_.MediaType)
        Add-ReportLine ''
    }
} catch { Add-ReportLine ('Disk query unavailable: ' + $_.Exception.Message) }
try {
    $physical = Get-PhysicalDisk -ErrorAction Stop
    foreach ($disk in $physical) {
        Add-ReportLine ('Storage API model: ' + $disk.FriendlyName)
        Add-ReportLine ('Media type (HDD/SSD where supported): ' + $disk.MediaType)
        Add-ReportLine ('Health status: ' + $disk.HealthStatus)
        Add-ReportLine ('Operational status: ' + ($disk.OperationalStatus -join ', '))
        Add-ReportLine ''
    }
} catch { Add-ReportLine 'Storage Spaces physical-disk details: not exposed by this Windows build/device or access level' }

Add-Section 'LOGICAL VOLUMES'
try {
    Get-CimInstance -ClassName Win32_LogicalDisk -Filter 'DriveType=3' -ErrorAction Stop | ForEach-Object {
        Add-ReportLine ('Drive: ' + $_.DeviceID)
        Add-ReportLine ('File system: ' + $_.FileSystem)
        Add-ReportLine ('Size: ' + (Format-GB $_.Size))
        Add-ReportLine ('Free: ' + (Format-GB $_.FreeSpace))
        Add-ReportLine ''
    }
} catch { Add-ReportLine ('Volume query unavailable: ' + $_.Exception.Message) }

Add-Section 'DISPLAY, AUDIO, NETWORK AND BATTERY DEVICES'
try {
    Get-CimInstance -ClassName Win32_SoundDevice -ErrorAction Stop | ForEach-Object { Add-ReportLine ('Audio: ' + $_.Name + ' | Status: ' + $_.Status) }
} catch { Add-ReportLine 'Audio inventory unavailable' }
try {
    Get-CimInstance -ClassName Win32_NetworkAdapter -ErrorAction Stop | Where-Object { $_.PhysicalAdapter -eq $true } | ForEach-Object {
        Add-ReportLine ('Network adapter: ' + $_.Name + ' | Manufacturer: ' + $_.Manufacturer + ' | Status: ' + $_.NetConnectionStatus)
    }
} catch { Add-ReportLine 'Network adapter inventory unavailable' }
try {
    Get-CimInstance -ClassName Win32_Battery -ErrorAction Stop | ForEach-Object {
        Add-ReportLine ('Battery: ' + $_.Name + ' | Status: ' + $_.Status + ' | Estimated charge: ' + $_.EstimatedChargeRemaining + '%')
    }
} catch { Add-ReportLine 'Battery inventory unavailable or no battery is reported' }

Add-Section 'DEVICE MANAGER PROBLEMS'
try {
    $problemDevices = Get-CimInstance -ClassName Win32_PnPEntity -ErrorAction Stop | Where-Object { $_.ConfigManagerErrorCode -ne 0 }
    if ($problemDevices) {
        $problemDevices | Select-Object -First 30 | ForEach-Object { Add-ReportLine ('Device: ' + $_.Name + ' | Error code: ' + $_.ConfigManagerErrorCode) }
    } else { Add-ReportLine 'No devices with a non-zero ConfigManagerErrorCode were reported.' }
} catch { Add-ReportLine ('Device status query unavailable: ' + $_.Exception.Message) }

Add-Section 'STARTUP INVENTORY (NAMES ONLY; NO COMMANDS OR PATHS)'
$startupNames = New-Object System.Collections.Generic.List[string]
$runKeys = @(
    'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run',
    'HKLM:\Software\Microsoft\Windows\CurrentVersion\Run',
    'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run'
)
foreach ($key in $runKeys) {
    try {
        if (Test-Path $key) {
            $item = Get-ItemProperty -Path $key -ErrorAction Stop
            foreach ($property in $item.PSObject.Properties) {
                if ($property.Name -notlike 'PS*') { [void]$startupNames.Add($property.Name) }
            }
        }
    } catch { }
}
$startupFolders = @(
    [Environment]::GetFolderPath('Startup'),
    [Environment]::GetFolderPath('CommonStartup')
)
foreach ($folder in $startupFolders) {
    try {
        if (Test-Path $folder) {
            Get-ChildItem -LiteralPath $folder -File -ErrorAction SilentlyContinue | ForEach-Object { [void]$startupNames.Add($_.BaseName) }
        }
    } catch { }
}
if ($startupNames.Count -eq 0) { Add-ReportLine 'No startup names were discovered or this area is inaccessible.' }
else { $startupNames | Sort-Object -Unique | ForEach-Object { Add-ReportLine $_ } }

Add-Section 'PROCESS SUMMARY'
try {
    $allProcesses = @(Get-Process -ErrorAction Stop)
    Add-ReportLine ('Total visible processes: ' + $allProcesses.Count)
    Add-Section 'TOP PROCESSES BY WORKING SET (NAMES AND MEMORY ONLY)'
    $allProcesses | Sort-Object WorkingSet64 -Descending | Select-Object -First 12 | ForEach-Object {
        Add-ReportLine ('{0,-28} {1,8:N0} MB' -f $_.ProcessName, ($_.WorkingSet64 / 1MB))
    }
} catch { Add-ReportLine ('Process inventory unavailable: ' + $_.Exception.Message) }

Add-Section 'POWER PLAN'
try {
    $activePlan = (& powercfg.exe /getactivescheme 2>&1 | Out-String).Trim()
    Add-ReportLine $activePlan
} catch { Add-ReportLine 'Active power plan unavailable' }

Add-Section 'END OF REPORT'
Add-ReportLine 'This script did not change Windows configuration.'
Add-ReportLine 'No services, startup entries, security settings, drivers, partitions, boot settings, or user files were modified.'

$reportText = $lines -join [Environment]::NewLine
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
    if (Test-Path -LiteralPath $resolvedOutput) {
        throw "Refusing to overwrite an existing report file: $resolvedOutput"
    }
    $parent = Split-Path -Parent $resolvedOutput
    if (-not (Test-Path -LiteralPath $parent)) {
        throw "Output folder does not exist. Create it yourself first: $parent"
    }
    [System.IO.File]::WriteAllText($resolvedOutput, $reportText, (New-Object System.Text.UTF8Encoding($true)))
    Write-Output ('Report saved to: ' + $resolvedOutput)
}
else {
    Write-Output $reportText
}
