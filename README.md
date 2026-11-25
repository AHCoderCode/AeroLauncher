# AeroOS Windows Edition

**AeroOS Windows Edition 0.1.0** is a lightweight, native WPF desktop companion for Windows 10. Windows remains the actual operating system: AeroOS runs in a normal resizable app window and does not replace Windows Explorer, the taskbar, Winlogon, the Windows shell, the bootloader or any installed driver.

> **Windows-preservation rule:** this project does not erase, format, repartition, reset, reinstall or replace Windows. It is not an OS installer or a Windows debloater.

## Current implementation

- Native WPF desktop window with Overview, Applications, Personalization, Control Center and About pages.
- Discovers `.lnk` app shortcuts from the current user's and all-users' Start Menu folders.
- Searches discovered app names/categories, launches shortcuts through the Windows shell, and stores AeroOS pins/recent launches.
- Opens the real Windows File Explorer, Task Manager and native Settings pages.
- Reads basic CPU, GPU, memory, Windows and system-drive information through WMI.
- Dark/light app theme, accent selection and AeroOS-only workspace background image.
- AeroOS-only Performance Mode that suppresses the content background image; it does not tune Windows services or global performance settings.
- User-level installation/uninstallation scripts with marker/path checks, no administrator requirement and no startup persistence.
- Read-only PowerShell hardware audit script.
- GitHub Actions Windows build workflow.

## Explicitly not implemented

This first build does **not** replace the Windows desktop or taskbar, provide system-tray integration, manage notifications, change Windows wallpaper/theme, alter Windows power plans, disable services/startup apps, install drivers, apply undocumented registry tweaks, or claim measured performance gains. Those features require separate design and physical-device tests and are intentionally not simulated.

## Hardware context

The existing Linux AeroOS repository README documents an HP ProBook 4520s target with an Intel Core i5-480M, 8 GB RAM, Legacy BIOS and a user-reported Radeon HD 64xx-family GPU. Those are repository-documented target details, not a live scan of the Windows installation. Run the included diagnostic report to confirm the actual hardware before making any optimization recommendations.

## Quick start

1. Build on Windows with Visual Studio 2022, the .NET desktop development workload and the .NET Framework 4.8 Developer Pack.
2. Run `scripts\Build-Release.ps1` from a normal PowerShell window.
3. Extract `artifacts\AeroOS-Windows-v0.1.0.zip`.
4. Run `Install-User.ps1` without Administrator privileges.
5. Launch AeroOS from the Start Menu.

See [docs/PHASE-1-REPORT.md](docs/PHASE-1-REPORT.md), [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), [docs/INSTALL.md](docs/INSTALL.md), [docs/SAFETY.md](docs/SAFETY.md), [docs/TEST-PLAN.md](docs/TEST-PLAN.md) and [docs/CURRENT-COMPATIBILITY.md](docs/CURRENT-COMPATIBILITY.md).

## Build output through GitHub Actions

After this source is pushed to `main`, the **Build AeroOS Windows Edition** workflow compiles on a Windows runner and uploads `AeroOS-Windows-v0.1.0.zip` as a 14-day workflow artifact. The workflow does not publish a public release or deploy anything.

## Project layout

```text
AeroOS-Windows/
├── .github/workflows/build-windows.yml
├── docs/
├── scripts/
│   ├── Build-Release.ps1
│   ├── Get-AeroOSHardwareReport.ps1
│   ├── Install-User.ps1
│   └── Uninstall-User.ps1
├── src/AeroOS.Windows/
│   ├── Models/
│   ├── Services/
│   ├── App.xaml
│   ├── MainWindow.xaml
│   └── AeroOS.Windows.csproj
└── AeroOS.sln
```

## Security and privacy

No telemetry, analytics, account sign-in, password capture, or network service is included. Application preferences are stored at `%LOCALAPPDATA%\AeroOSWindows\settings.json`. Review scripts before running them. The current installer is unsigned.

## Status

Initial source implementation prepared. The project has not yet been built or run on Windows in this environment. The WPF application, diagnostic script, installer and uninstaller still require validation on the HP ProBook itself. No performance benchmark is claimed.
