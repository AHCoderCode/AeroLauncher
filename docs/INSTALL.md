# Installation on Windows 10 Pro

## Requirements

- Windows 10 64-bit.
- .NET Framework 4.8 Runtime. Many updated Windows 10 systems already include it. If the app reports a missing runtime, obtain the official .NET Framework 4.8 Runtime from Microsoft; do not use third-party driver or runtime download sites.
- For building from source: Visual Studio 2022 with the **.NET desktop development** workload and .NET Framework 4.8 Developer Pack/targeting pack.

## Build from source

1. Download or clone this repository to a normal folder such as `Documents\AeroOS-Windows`.
2. Open the folder in Visual Studio 2022.
3. Open `AeroOS.sln`.
4. Select `Release` and `Any CPU` in the toolbar.
5. Build > Build Solution.
6. When the build succeeds, run `scripts\Build-Release.ps1` from a normal PowerShell window to create the portable bundle. Do not run it as Administrator.

Alternatively, open PowerShell in the repository folder and run:

```powershell
.\scripts\Build-Release.ps1
```

The output is `artifacts\AeroOS-Windows-v0.1.0.zip`.

## Install for the current Windows user

1. Extract the zip to a normal folder.
2. Open PowerShell in that extracted folder.
3. Run `.\Install-User.ps1` (without Administrator privileges).
4. Answer `Y` or `N` when asked whether to create a Desktop shortcut.
5. Open AeroOS from the Start Menu.

The installer refuses to overwrite an existing AeroOS folder. It installs to `%LOCALAPPDATA%\Programs\AeroOSWindows` and creates a Start Menu shortcut. It does not add an automatic startup entry.

## Uninstall

1. Close AeroOS normally.
2. Open `%LOCALAPPDATA%\Programs\AeroOSWindows` in File Explorer.
3. Right-click `Uninstall-AeroOS.ps1` and choose **Run with PowerShell**, or open a normal PowerShell window in that folder and run `.\Uninstall-AeroOS.ps1`.
4. Type `REMOVE AEROOS` when prompted.

The uninstaller removes only the validated AeroOS program folder and the AeroOS-named shortcuts. It leaves `%LOCALAPPDATA%\AeroOSWindows` preferences in place. It does not remove other applications or Windows components.

## Read-only hardware report

Open PowerShell in the repository folder and run:

```powershell
.\scripts\Get-AeroOSHardwareReport.ps1
```

This prints a report without changing system configuration. To save a new report file on the Desktop:

```powershell
.\scripts\Get-AeroOSHardwareReport.ps1 -OutputPath "$env:USERPROFILE\Desktop\AeroOS-Hardware-Report.txt"
```

The script refuses to overwrite an existing report file. Do not share the report publicly without reviewing it first.

## Windows 10 security-support note

Windows 10 normal support ended on October 14, 2025. Eligible Windows 10 22H2 personal devices may enroll in Microsoft's Consumer Extended Security Updates program for critical and important security updates through October 12, 2027. Microsoft lists additional prerequisites, including current updates, KB5126256, and an administrator Microsoft account that is not a child account. Program availability can vary by market. Review the official [Microsoft ESU page](https://www.microsoft.com/en-us/windows/extended-security-updates) with the account holder/parent or guardian where applicable. AeroOS does not enroll the device, change Windows Update, or bypass account restrictions.

## Rollback

Close AeroOS and run its uninstaller. If installation failed before the Start Menu shortcut was created, inspect the exact `%LOCALAPPDATA%\Programs\AeroOSWindows` folder and remove only that AeroOS-owned folder after confirming it contains no personal files. The app does not take over Windows startup or the shell, so closing/uninstalling it returns you to the normal Windows desktop.
