# AeroOS Windows Edition

**AeroOS Windows Edition 0.3.0** is a custom, full-screen desktop launcher for Windows 10. It is a standalone Windows application project in the **AeroLauncher** repository; it is separate from the Linux Aero-Os repository and separate from any AeroShell Desktop project.

AeroOS gives the user a custom glass-style workspace with its own visual taskbar, Start panel, app search, quick-launch tiles, folder shortcuts, clock, quick settings links and personalization. It launches the existing Windows applications rather than replacing them.

## Windows preservation comes first

AeroOS runs as one ordinary, borderless, maximized WPF window. It does **not** replace Windows Explorer, the Windows shell, Winlogon, the native taskbar, the bootloader or any driver. It does not add login startup, services, scheduled tasks, registry shell changes, power-plan tweaks or background persistence. Closing AeroOS with its Exit button or Alt+F4 closes the app and reveals the normal Windows desktop.

The project must never erase, format, repartition, reset, reinstall or replace Windows. It is not an operating-system installer or Windows debloater.

## 0.3.0 launcher features

- Full-screen, custom AeroOS workspace with layered glass gradients, luminous edge highlights and static ambient colour glows.
- Rounded glass control templates with consistent hover, press and focus states.
- In-app Start panel with search for discovered Start Menu shortcuts and built-in Windows tools.
- Quick-launch tiles for File Explorer, the default web browser, Windows Settings, Calculator, Paint and Notepad.
- Shortcuts to the user's Home, Documents, Downloads, Pictures, Music, This PC and Recycle Bin locations.
- Links to native Windows network, display, sound and Bluetooth settings.
- System clock and date, plus a best-effort, one-time hardware summary.
- AeroOS-only dark/light appearance, accent colours and optional workspace wallpaper.
- Reduced-effects mode that disables the custom workspace wallpaper and keeps the visual surface static.
- No account, telemetry, analytics, password capture or background network service.

## Performance design

The interface uses WPF and built-in .NET Framework libraries. It has no continuous animation loop. The clock refreshes once per minute. Start Menu discovery and the basic hardware summary happen once after launch. Reduced-effects mode avoids decoding and displaying the optional wallpaper.

These are resource-conscious design choices, **not measured battery-life, RAM, CPU or FPS guarantees**. Actual impact must be tested on the target laptop.

## Compatibility and build

- Target: Windows 10, 64-bit.
- Framework: .NET Framework 4.8.
- UI: native WPF; no Electron or embedded browser runtime.
- Hardware target: HP ProBook 4520s / 8 GB RAM, with exact processor and graphics model to be confirmed on the device.

The GitHub Actions workflow builds the application on a Windows runner and uploads a portable ZIP artifact. The bundle contains the compiled application files and information text only. It does not package PowerShell installer or uninstaller scripts. The current application is unsigned, and a Windows build and device test are still required before it should be used as a daily desktop.

## Explicit limitations

AeroOS does not provide a replacement operating system, system-wide shell replacement, native Windows taskbar replacement, a new kernel, a custom driver stack, its own browser engine, or a custom file-system implementation. File Explorer, browser, Settings and other system tools are the real Windows applications opened by AeroOS. Some Start Menu shortcuts and Store apps may not be discoverable as ordinary shortcut files.

## Privacy and settings

The application stores its own preferences under:

%LOCALAPPDATA%\AeroOSWindows\settings.json

The optional wallpaper path is used only by the AeroOS window. It does not change Windows wallpaper. App discovery reads Start Menu shortcut names and paths locally.

## Build status

Source implementation for 0.3.0 has been committed. The glass visual refresh must pass the Windows CI build and then be checked interactively on the HP ProBook for layout, controls, responsiveness and performance. No malware-free certification or measured performance result is claimed.
