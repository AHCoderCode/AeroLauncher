# Architecture

## Execution model

`AeroOS.Windows.exe` is a standard WPF desktop process targeting .NET Framework 4.8. It is launched by the user and runs in a normal Windows window. Closing it leaves the pre-existing Windows desktop and shell in place.

## Components

- `ApplicationCatalog`: enumerates `.lnk` shortcuts under the current user's and all-users' Programs Start Menu folders; does not scan user documents or crawl the entire disk.
- `SettingsLauncher`: opens native `ms-settings:` pages with Control Panel fallbacks where available.
- `SystemInfoService`: reads basic CPU, GPU, memory and system-drive inventory through WMI and `DriveInfo`.
- `PreferencesService`: stores theme, accent, app-only background path, pins and recent shortcuts in `%LOCALAPPDATA%\AeroOSWindows\settings.json`.
- `MainWindow`: Overview, Applications, Personalization, Control Center and About views.
- `Get-AeroOSHardwareReport.ps1`: separate read-only inventory for deeper Windows/hardware checks.
- `Install-User.ps1` / `Uninstall-User.ps1`: user-profile-only program copy and shortcut management.

## Trust boundaries

The app does not run as a service, request administrator privileges, edit global registry settings, replace the shell, or alter boot configuration. Launching a shortcut delegates execution to the Windows shell with `UseShellExecute=true`. Settings navigation delegates to Windows Settings or Control Panel.

The installer creates a folder under the current user's LocalAppData Programs directory and AeroOS-named shortcuts. It does not register a run-at-logon entry. The uninstaller checks an exact path and marker before removing only the AeroOS program directory; it deletes a shortcut only when its target still points to the AeroOS executable. Preferences are separate and retained.

## Resource strategy

No embedded browser, daemon, tray poller or continuous hardware polling is used. App discovery and WMI reads occur on background tasks. The only periodic UI timer updates the in-window clock once per minute. Performance mode skips the decorative app-only background image; it does not change global Windows behavior.
