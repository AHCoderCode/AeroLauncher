# AeroOS Windows Edition

A lightweight native Windows desktop companion for the HP ProBook 4520s target. Windows remains the actual operating system.

> **Strict Windows-preservation rule:** AeroOS must never erase, format, repartition, reset, reinstall or replace the existing Windows installation. It must not modify boot files, services, drivers, security settings or global registry configuration.

## Add the source package and build the app

The repository is initialized. To import and build the complete WPF project:

1. Download the source ZIP from the ChatGPT conversation attachment named `AeroOS-Windows-source-v0.1.0.zip`.
2. Open this repository on GitHub.
3. Select **Add file → Upload files**.
4. Upload `AeroOS-Windows-source-v0.1.0.zip` into the repository root (not into a folder).
5. Commit the upload to `main`.
6. Open the **Actions** tab and select **Import, build and package AeroOS Windows Edition**.
7. Open the latest run. Wait until every step has a green check and the run says **Success**.
8. In the run's **Artifacts** area, download `AeroOS-Windows-v0.1.0`.

The import workflow extracts the source and builds it on a GitHub-hosted Windows runner. The resulting workflow artifact contains another ZIP called `AeroOS-Windows-v0.1.0.zip`. Extract that inner ZIP to a normal folder. It contains `Install-User.ps1`, `Uninstall-User.ps1`, and a `publish` folder. Follow `docs/INSTALL.md` for installation.

If a run fails and a workflow fix is committed afterward, start a **new** run from the updated workflow: open **Actions → Import, build and package AeroOS Windows Edition → Run workflow**, select `main`, then click **Run workflow**. Do not use **Re-run jobs** for an old run when its workflow definition itself has changed; that can replay the previous revision.

Do not download or install an artifact from a failed run.

## Architecture

- Native WPF app targeting .NET Framework 4.8.
- Start Menu shortcut discovery and native Windows application launching.
- AeroOS-only preferences, theme, accent, pins and recent apps.
- Real Explorer, Task Manager and Windows Settings integrations.
- Read-only WMI/PowerShell hardware inventory.
- User-profile-only installer and guarded uninstaller.
- GitHub Actions Windows build workflow.

## Safety boundaries

AeroOS runs in a normal resizable window. It does not replace Explorer, the taskbar, Winlogon or the Windows shell. It has no service, bootloader, partitioning or global system-tuning component. Installation is user-level and creates only AeroOS-owned program files and shortcuts. Windows files, existing applications, drivers, services, boot configuration and user documents are outside the installer's scope.

The included diagnostic script is read-only. Do not apply optimization changes based on assumptions; inspect the actual hardware report first.

## Existing Linux AeroOS

The separate [AHCoderCode/Aero-Os](https://github.com/AHCoderCode/Aero-Os) repository remains untouched. This Windows Edition is an independent project.

## Validation status

The source package's XML and ZIP structure were checked in the preparation workspace. The project still needs a successful Windows-runner build, then physical HP ProBook testing of UI behavior, devices and performance. No measured performance gain is claimed.