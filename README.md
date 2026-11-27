# AeroShell Desktop

**AeroShell Desktop 0.8.0** is a Windows 11-inspired desktop companion for Windows 10. It is a portable WPF application that runs inside a normal Windows window. It is not a replacement operating system or system shell.

## Windows preservation comes first

AeroShell does not replace Windows Explorer, the Windows shell, Winlogon, the native taskbar, the bootloader or any driver. It does not add login startup, services, scheduled tasks, registry shell changes, power-plan tweaks or background persistence. Close the window or choose Exit AeroShell to return to the normal Windows desktop.

The project must never erase, format, repartition, reset, reinstall or replace Windows. It is not an operating-system installer or Windows debloater.

## 0.8.0 desktop refresh

- Removed the permanent left navigation rail and expanded the desktop workspace.
- Rebuilt the bottom dock as a centered, icon-only taskbar with hover labels.
- Added a four-pane Windows-style Start icon and placed the Start panel above the dock.
- Added Fluent-style Windows system glyphs, brighter glass surfaces, layered ambient lighting, soft shadows and short hover-lift effects.
- Reworked the desktop welcome area and app shortcuts.
- Corrected the Start panel's mouse handling so clicking inside the search field does not get swallowed.
- Reworked the text-box template so padding no longer consumes the scrolling text viewport; search fields are single-line and keep the caret-positioned text visible.
- Kept Desktop, All apps, Files & folders and Personalization accessible from the centered taskbar and Start panel.
- Kept normal Windows minimize, maximize/restore, resize and close controls.

## Navigation

The centered taskbar provides Start, Desktop, File Explorer, Browser, All apps, Files & folders, Personalization, Calculator, Paint, Notepad and Exit AeroShell. Hover over an icon to see its name.

| Shortcut | Action |
|---|---|
| Ctrl + 1 | Desktop |
| Ctrl + 2 | All apps |
| Ctrl + 3 | Files & folders |
| Ctrl + 4 | Personalization |
| Ctrl + Space | Open/close Start |
| Ctrl + , | Open Personalization |
| Esc | Close Start |
| Alt + F4 | Close AeroShell and return to Windows |

## Launcher capabilities

- Search for discovered Start Menu shortcuts and built-in Windows tools.
- Pin and unpin up to 12 applications.
- Remember up to eight recently opened applications locally.
- Launch File Explorer, the default web browser, Windows Settings, Calculator, Paint, Notepad, Task Manager and Control Panel.
- Open common user folders, This PC and Recycle Bin in the real Windows File Explorer.
- Open native Windows network, display, sound and Bluetooth settings.
- Save AeroShell appearance and app preferences locally.
- Choose between dark/light appearance, accent colours, optional wallpaper and reduced effects.

## Performance and compatibility

- Target: Windows 10, 64-bit.
- Framework: .NET Framework 4.8.
- UI: native WPF; no Electron or embedded browser runtime.
- Intended hardware class includes older laptops with integrated or entry-level graphics and 8 GB RAM.
- Effects are layered gradients, shadows and brief hover animations. There is no continuous animation loop or permanent live-blur pipeline.

Full visual effects are enabled by default, including when upgrading an older profile whose previous default was Reduced Effects. Reduced Effects remains available in Personalization for older graphics hardware. The one-time migration only updates AeroShell's own preference; a new Reduced Effects choice is saved normally. The richer look may use more graphics resources than the previous low-effects layout. Actual CPU, RAM, battery and GPU usage must be measured on the target HP ProBook 4520s; no performance guarantee or target-laptop runtime test is claimed.

## Security and limitations

The app is currently **unsigned**. A successful build, matching checksum or clean antivirus scan is not a malware-free certification. Review and scan the package before running it. Do not bypass Microsoft Defender or SmartScreen warnings. AeroShell should not require administrator privileges; cancel if an unexpected UAC prompt appears.

AeroShell is a desktop companion, not a replacement operating system, native Windows taskbar, system-wide shell or custom file manager. File Explorer, browser, Settings and other system tools are the real Windows applications opened by AeroShell. Installed apps are discovered from ordinary Windows Start Menu shortcuts; some packaged apps may not appear automatically.

## Privacy and settings

The application stores its own preferences under:

%LOCALAPPDATA%\AeroOSWindows\settings.json

The optional wallpaper path is used only by the AeroShell window. It does not change Windows wallpaper. App discovery reads Start Menu shortcut names and paths locally. The source does not implement account login, telemetry, analytics, password collection or background network services.

## Validation status

Version 0.8.0 changes the desktop layout, taskbar, Start panel, icon styling and text input template. GitHub Actions verifies source checks, compilation and packaging. Interactive UI behaviour, display scaling, security scan and performance still require testing on the target HP laptop; no laptop runtime test is claimed.
