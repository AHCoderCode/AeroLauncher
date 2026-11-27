# AeroShell Desktop

**AeroShell Desktop 0.7.0** is a Windows 12-inspired desktop companion for Windows 10. It is a portable WPF application in the **AeroLauncher** repository. It provides a desktop-style workspace inside an ordinary app window; it is not a replacement operating system.

## Windows preservation comes first

AeroShell runs as one ordinary maximized, resizable WPF window with the normal Windows title-bar controls. It does **not** replace Windows Explorer, the Windows shell, Winlogon, the native taskbar, the bootloader or any driver. It does not add login startup, services, scheduled tasks, registry shell changes, power-plan tweaks or background persistence. Close the window or use Exit to return to the normal Windows desktop.

The project must never erase, format, repartition, reset, reinstall or replace Windows. It is not an operating-system installer or Windows debloater.

## 0.7.0 desktop and navigation

- Desktop-style shortcut surface with one-click access to This PC, Recycle Bin, File Explorer, browser, common folders, Settings, Calculator, Paint, Notepad and Task Manager.
- Persistent left navigation for Desktop, All apps, Files & folders and Personalization.
- Centered taskbar with Start, app launchers, page navigation and a dedicated Exit control.
- Start panel with app search, pinned apps and recent applications.
- Dedicated All apps page with live search, category matching, app launch and pin/unpin controls.
- Dedicated Files & folders page with Home, Documents, Downloads, Pictures, Music, This PC and Recycle Bin shortcuts.
- Dedicated Personalization page for dark/light appearance, violet/blue/teal/rose accents, reduced effects and AeroShell-only wallpaper.
- Normal Windows title-bar minimize, maximize/restore and close controls; the app starts maximized but can be resized.
- Keyboard navigation shortcuts for Desktop, Apps, Files and Personalization.
- Lightweight translucent panels and static ambient gradients; no continuous animation loop or expensive live blur.

## Keyboard shortcuts

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
- Reduced-effects mode avoids decoding and displaying the optional wallpaper.

## Performance and compatibility

- Target: Windows 10, 64-bit.
- Framework: .NET Framework 4.8.
- UI: native WPF; no Electron or embedded browser runtime.
- Intended hardware class: older laptops with integrated or entry-level graphics and 8 GB RAM.
- The desktop is designed to avoid continuous animation and live blur. Actual CPU, RAM, battery and GPU use must still be measured on the HP ProBook 4520s.

These are resource-conscious design choices, **not measured battery-life, RAM, CPU or FPS guarantees**.

## Security and limitations

The app is currently **unsigned**. A successful build, matching checksum or clean antivirus scan is not a malware-free certification. Review and scan the package before running it. Do not bypass Microsoft Defender or SmartScreen warnings. AeroShell should not require administrator privileges; cancel if an unexpected UAC prompt appears.

AeroShell is a desktop companion, not a replacement operating system, native Windows taskbar, system-wide shell or custom file manager. File Explorer, browser, Settings and other system tools are the real Windows applications opened by AeroShell. Some Start Menu shortcuts and Store apps may not be discoverable as ordinary shortcut files.

## Privacy and settings

The application stores its own preferences under:

%LOCALAPPDATA%\AeroOSWindows\settings.json

The optional wallpaper path is used only by the AeroShell window. It does not change Windows wallpaper. App discovery reads Start Menu shortcut names and paths locally. The source does not implement account login, telemetry, analytics, password collection or background network services.

## Validation status

Version 0.7.0 adds a desktop shortcut surface, a dedicated Personalization page, simpler navigation and standard window controls. GitHub Actions can verify source checks, compilation and packaging. Interactive UI behaviour, display scaling, security scan and performance still require testing on the target HP laptop; no laptop runtime test is claimed.
