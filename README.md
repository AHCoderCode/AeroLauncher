# AeroOS Windows Edition

**AeroOS Windows Edition** is a lightweight native Windows desktop companion for the HP ProBook 4520s target. It runs as an ordinary Windows application; Windows remains the operating system.

> **Strict preservation guarantee:** AeroOS is not an OS installer, reset tool, debloater or shell replacement. It does not erase, format, repartition, reinstall or replace Windows. It does not modify boot files, services, drivers, Windows security settings or global registry configuration.

## Source package

The complete initial project source (WPF application, PowerShell scripts, documentation, installer/uninstaller and Windows CI workflow) is included in:

- [AeroOS Windows source v0.1.0 ZIP](./AeroOS-Windows-source-v0.1.0.zip)
- [SHA-256 checksum](./AeroOS-Windows-source-v0.1.0.sha256)

After extracting the ZIP, open the `AeroOS-Windows` folder. See its README for build and beginner installation steps.

## Project architecture

- Native WPF desktop application targeting .NET Framework 4.8.
- Start Menu shortcut discovery and native Windows application launching.
- AeroOS-only preferences, theme, accent, pinned apps and recent apps.
- Real Explorer, Task Manager and Windows Settings integration.
- Read-only WMI/PowerShell hardware inventory.
- User-profile-only installer and guarded uninstaller.
- GitHub Actions Windows build workflow.

## Safety boundary

AeroOS is an additional app in a normal resizable window. It does not replace Explorer, the Windows taskbar, Winlogon or the Windows shell. It has no service, bootloader, partitioning or global system-tuning component. Installation is user-level and creates only AeroOS-owned program files and shortcuts. Preferences are retained after uninstall.

Do not run system optimization changes based on assumptions. First run the included read-only hardware report and inspect its results. Never select the internal Windows drive for any unrelated OS/USB imaging operation.

## Current validation status

The source package's XAML/project XML and ZIP structure were checked. A Windows runner must compile the WPF project and run the Windows-specific tests. The target laptop must still be physically tested for GPU, Wi-Fi, Bluetooth, audio, battery, sleep/resume and performance. No benchmark claims are made.

## Existing Linux AeroOS

The separate [AHCoderCode/Aero-Os](https://github.com/AHCoderCode/Aero-Os) repository remains untouched. This Windows Edition is a separate codebase.