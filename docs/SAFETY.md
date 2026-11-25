# Safety and Windows-preservation contract

AeroOS Windows Edition is a normal desktop application. It is not an operating-system installer, shell replacement, Windows optimizer, driver manager or disk utility.

## Protected Windows components

The application and its included scripts must not:

- erase, format, repartition, reset, reinstall or replace Windows;
- replace, modify or disable the Windows bootloader or boot configuration;
- replace or terminate Explorer.exe as a shell-management action;
- modify Windows services, Defender, Firewall, Windows Update or security policy;
- install, remove or force drivers;
- flash firmware or change BIOS/UEFI settings;
- change global registry settings or Windows power plans;
- move, rename, alter permissions on, or delete user documents;
- install a hidden background service or automatic logon persistence.

## What the application does

- Reads user and all-users Start Menu `.lnk` shortcuts.
- Starts a selected shortcut through the normal Windows shell.
- Opens Windows File Explorer, Task Manager and native Settings pages.
- Reads basic hardware inventory through WMI.
- Saves only AeroOS preferences under `%LOCALAPPDATA%\AeroOSWindows`.
- Displays a selected image only inside the AeroOS window. It does not change the Windows wallpaper.

## Installation boundary

The optional installer copies program files to `%LOCALAPPDATA%\Programs\AeroOSWindows` and creates named Start Menu/Desktop shortcuts. It does not require elevation and does not add a startup entry. The uninstaller verifies both the exact expected folder and an AeroOS marker before removing that AeroOS program folder and its two named shortcuts. Preferences are deliberately retained.

## Diagnostic script

`Get-AeroOSHardwareReport.ps1` gathers read-only information. It displays the report in the console unless an explicit new output path is supplied. It refuses to overwrite an existing report file. It deliberately omits serial numbers, IP/MAC addresses, account names and startup command lines.

## Known limits

- The app is a Windows 10-compatible WPF window, not a system-wide desktop shell or native taskbar replacement.
- Application discovery depends on Start Menu shortcuts. Some Store apps or portable applications may not appear.
- Some `ms-settings:` pages vary by Windows 10 build. The app uses a Control Panel fallback where practical.
- The Performance Mode switch affects only AeroOS's own decorative workspace background; it does not optimize Windows system services or promise an FPS increase.
- Hardware information is best-effort WMI data and must be checked against the actual laptop.
- The installer is unsigned. Review the source before running it.
