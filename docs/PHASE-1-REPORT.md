# Phase 1 — repository and hardware investigation

Date: 2026-11-25

## Repository findings

### Existing Linux AeroOS

- Repository: https://github.com/AHCoderCode/Aero-Os
- Visibility: public
- Default branch: `main`
- Repository metadata reports owner-level push/admin permissions for the connected GitHub account.
- The README describes AeroOS 0.4.1-rc1 as a Debian 13 Live system using Xfce and LightDM. It includes a Live ISO, build workflow and Linux-specific configuration/scripts.
- The README documents the HP ProBook 4520s target profile as Intel Core i5-480M, 8 GB RAM, Legacy BIOS, and a user-reported Radeon HD 64xx-family GPU.
- It explicitly distinguishes successful CI ISO checks from physical hardware certification.

**Action:** no files, branches, workflows, releases or history in this Linux repository were modified.

### New Windows repository

- Requested repository: https://github.com/AHCoderCode/AeroOS-Windows
- GitHub API returned `404 Not Found`; the owner repository listing did not contain it.
- The available GitHub integration exposes repository read/write operations for existing repositories but does not expose a repository-creation operation.

**Action:** implementation source was prepared separately. Publishing requires creating the new repository once in GitHub, then pushing this source bundle. The existing Linux project was not used as a target.

## Hardware information and confidence

| Item | Current information | Confidence |
|---|---|---|
| Laptop | HP ProBook 4520s | User-provided project target |
| CPU | Intel Core i5-480M | Listed in existing Linux AeroOS README; must confirm from Windows |
| RAM | 8 GB | User-provided and Linux README target profile |
| GPU | Radeon HD 64xx family | User-reported family in Linux README; exact adapter/PCI ID unknown |
| Firmware | Legacy BIOS | Listed in Linux README; must confirm in Windows |
| Storage | Unknown HDD/SSD model and health | Requires Windows report |
| Windows | Windows 10 Pro x64 | User-provided; exact release/build not yet confirmed |
| Wi-Fi/Bluetooth/audio | Exact adapter and driver state unknown | Requires Windows report and device tests |

The repository's target profile is useful for choosing a conservative WPF/.NET Framework desktop application, but it is not a live reading of the current Windows installation.

## Read-only hardware audit plan

Run `scripts/Get-AeroOSHardwareReport.ps1` in ordinary (non-Administrator) Windows PowerShell. It collects OS/build, CPU, GPU/driver, memory, physical/logical storage, disk health where exposed, BIOS/Secure Boot/TPM availability, network/audio/battery devices, device-manager error codes, startup names, process working-set summary and current power plan. It omits machine serial numbers, MAC/IP addresses, account names and startup command lines.

The report is informational. No optimization should be applied until the exact Windows build, storage type/health, GPU/driver and startup entries are reviewed.

## Architecture decision

Build a Windows-native WPF application targeting .NET Framework 4.8. It runs as a normal resizable window and leaves Explorer, the taskbar, Winlogon, Windows drivers, services and boot configuration in control. The app uses built-in Windows APIs/WMI, shell shortcuts and native Settings URI launches; it has no web runtime, telemetry service or third-party package dependency.

## Known project constraints

The source bundle can be prepared in this environment, but WPF compilation and physical-device testing require Windows. This Linux workspace cannot execute the Windows WPF app or certify hardware compatibility. GitHub Actions is configured to build on a Windows runner once the source is pushed to the new repository.
