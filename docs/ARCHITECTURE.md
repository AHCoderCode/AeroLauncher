# AeroOS Windows Edition architecture

## Scope

AeroOS is a standalone custom desktop launcher window in the AeroLauncher repository. It is not AeroShell Desktop and is not the Linux Aero-Os distribution. The app is built to provide a visually distinct workspace while leaving the Windows installation and native shell intact.

## Layers

- **Presentation:** WPF XAML window and a code-built composition of translucent cards, panels, tiles, in-app Start and in-app taskbar dock.
- **Application discovery:** ApplicationCatalog enumerates Start Menu shortcut files from the current-user and common Programs folders.
- **Launch integration:** selected shortcuts and built-in Windows tools start through ProcessStartInfo with shell execution. No process injection, shell hooking or process termination is used.
- **Preferences:** PreferencesService reads/writes AeroOS-only JSON preferences in %LOCALAPPDATA%\AeroOSWindows.
- **Hardware summary:** SystemInfoService reads a one-time basic WMI snapshot; it is not a monitoring loop or telemetry service.

## Lifecycle

The user manually launches the executable. The single maximized window is displayed. A one-minute UI timer refreshes the clock. Start Menu discovery and hardware inventory are performed once after the window loads. Exit closes the process; it does not change the Windows shell or session.

## Resource strategy

No Electron runtime, remote web content, always-on background service, continuous animation loop, or recurring hardware polling is included. The optional wallpaper is decoded at a bounded width and is skipped in Reduced Effects mode.

## Limitations

The interface is a launcher shell inside an application window. It does not own Windows logon, the native desktop, the native taskbar or operating-system services. Windows Explorer and the actual Windows tools handle file browsing and system settings.
