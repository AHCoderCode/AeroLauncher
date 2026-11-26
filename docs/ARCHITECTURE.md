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

## 0.4.0 interaction model

- User pins and recent-app history are stored in the AeroOS preference JSON only.
- Start search covers the built-in catalog plus discovered Start Menu shortcuts, ranks prefix matches first and supports Enter-to-launch.
- Ctrl+Space opens/closes Start, Ctrl+Comma opens Personalize, and Escape closes the topmost overlay.
- The desktop collapses the optional right-hand information column on narrower layouts.
- Start Menu discovery is bounded by folder depth and result count and skips reparse-point directories.

## Limitations

The interface is a launcher shell inside an application window. It does not own Windows logon, the native desktop, the native taskbar or operating-system services. Windows Explorer and the actual Windows tools handle file browsing and system settings.
