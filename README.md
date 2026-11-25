# AeroOS Windows Edition

A lightweight native Windows desktop companion for the HP ProBook 4520s target. Windows remains the actual operating system.

> **Strict Windows-preservation rule:** AeroOS must never erase, format, repartition, reset, reinstall or replace the existing Windows installation. It must not modify boot files, services, drivers, security settings or global registry configuration.

## Project source

The complete starter source package is prepared as **AeroOS Windows Edition v0.1.0**. It contains the WPF application, build solution, scripts, installer/uninstaller, diagnostics, documentation, verification script and Windows CI workflow.

The package must be added to this repository once as `AeroOS-Windows-source-v0.1.0.zip`. A repository workflow then extracts the contents into the normal source tree and commits the files automatically.

### Add the source package (one file upload)

1. Download the prepared source ZIP from the ChatGPT conversation attachment.
2. Open the **AeroLauncher** repository on GitHub.
3. Select **Add file → Upload files**.
4. Upload the file named exactly `AeroOS-Windows-source-v0.1.0.zip` to the repository root (not inside a folder).
5. Commit the upload to `main`.
6. Open **Actions** and check **Import packaged AeroOS source**. It will unpack the source tree, including hidden configuration files, and commit the files.
7. The **Build AeroOS Windows Edition** workflow then runs on a Windows runner. Its artifact is available from the successful workflow run.

The import workflow checks for the expected archive and project folder before copying. It does not run any commands on your laptop or alter your Windows installation.

## Architecture

- Native WPF app targeting .NET Framework 4.8.
- Start Menu shortcut discovery and native Windows application launching.
- AeroOS-only preferences, theme, accent, pins and recent apps.
- Real Explorer, Task Manager and Windows Settings integrations.
- Read-only WMI/PowerShell hardware inventory.
- User-profile-only installer and guarded uninstaller.
- Windows build/verification workflow.

## Safety boundaries

AeroOS runs in a normal resizable window. It does not replace Explorer, the taskbar, Winlogon or the Windows shell. It has no service, bootloader, partitioning or global system-tuning component. Installation is user-level and creates only AeroOS-owned program files and shortcuts. Windows files, existing applications, drivers, services, boot configuration and user documents are outside the installer's scope.

The included diagnostic script is read-only. Do not apply optimization changes based on assumptions; inspect the actual hardware report first.

## Existing Linux AeroOS

The separate [AHCoderCode/Aero-Os](https://github.com/AHCoderCode/Aero-Os) repository remains untouched. This Windows Edition is an independent project.

## Validation status

Project XML and ZIP structure were checked in the preparation workspace. This Linux workspace cannot compile or execute WPF binaries. Windows compilation, runtime behavior and physical HP ProBook hardware/performance tests remain pending; no measured performance gain is claimed.