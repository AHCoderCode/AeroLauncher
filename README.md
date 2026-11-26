# AeroShell Desktop

**AeroShell Desktop 0.5.0** is a Windows 12-inspired desktop launcher for Windows 10. It is a portable WPF application in the **AeroLauncher** repository. It is a visual desktop experience, not a replacement operating system.

## Windows preservation comes first

AeroShell runs as one ordinary, maximized WPF window. It does **not** replace Windows Explorer, the Windows shell, Winlogon, the native taskbar, the bootloader or any driver. It does not add login startup, services, scheduled tasks, registry shell changes, power-plan tweaks or background persistence. Closing AeroShell with its Exit button or Alt+F4 reveals the normal Windows desktop.

The project must never erase, format, repartition, reset, reinstall or replace Windows. It is not an operating-system installer or Windows debloater.

## 0.5.0 desktop redesign

- Refreshed Windows 12-inspired desktop composition with a dark, layered colour field and restrained static ambient lighting.
- AeroShell brand identity and clearer product labeling.
- Refined navigation panel for Home, Documents, Downloads, Pictures, Music, This PC and Recycle Bin.
- Redesigned application tiles and search area.
- Centered floating taskbar-style dock with a dedicated, clearly labeled Exit control.
- Centered Start panel with application search, pinned Windows tools, recent apps and discovered Start Menu shortcuts.
- Redesigned personalization panel with appearance, accent, reduced-effects and AeroShell-only wallpaper options.
- Existing Windows application launching remains intact.
- Responsive layout collapses the optional device panel on narrower displays.

## Existing launcher features

- Search for discovered Start Menu shortcuts and built-in Windows tools.
- Pin and unpin up to 12 applications.
- Remember up to eight recently opened applications locally.
- Quick-launch File Explorer, default web browser, Windows Settings, Calculator, Paint and Notepad.
- Open Home, Documents, Downloads, Pictures, Music, This PC and Recycle Bin.
- Open native Windows network, display, sound and Bluetooth settings.
- Show local clock/date and best-effort hardware information.
- Save AeroShell appearance and app preferences locally.
- Reduced-effects mode avoids decoding and displaying the optional wallpaper.

## Performance design

The interface uses WPF and built-in .NET Framework libraries. It has no continuous animation loop. The clock refreshes once per minute. Start Menu discovery and the basic hardware summary happen after launch. Reduced-effects mode avoids decoding and displaying the optional wallpaper.

These are resource-conscious design choices, **not measured battery-life, RAM, CPU or FPS guarantees**. Actual impact must be tested on the HP ProBook 4520s.

## Compatibility and build

- Target: Windows 10, 64-bit.
- Framework: .NET Framework 4.8.
- UI: native WPF; no Electron or embedded browser runtime.
- Hardware target: HP ProBook 4520s / 8 GB RAM; exact processor and graphics model should be confirmed on the device.
- GitHub Actions builds the application on a Windows runner and uploads a portable ZIP artifact.

The app is currently **unsigned**. A successful build, matching checksum, or clean antivirus scan is not a malware-free certification. Review and scan the package before running it. Do not bypass Microsoft Defender or SmartScreen warnings.

## Explicit limitations

AeroShell does not provide a replacement operating system, system-wide shell replacement, native Windows taskbar replacement, a new kernel, a custom driver stack, its own browser engine, or a custom file-system implementation. File Explorer, browser, Settings and other system tools are the real Windows applications opened by AeroShell. Some Start Menu shortcuts and Store apps may not be discoverable as ordinary shortcut files.

## Privacy and settings

The application stores its own preferences under:

%LOCALAPPDATA%\AeroOSWindows\settings.json

The optional wallpaper path is used only by the AeroShell window. It does not change Windows wallpaper. App discovery reads Start Menu shortcut names and paths locally. The source does not implement account login, telemetry, analytics, password collection or background network services.

## Validation status

The 0.5.0 redesign is a source update. Its Windows build, static checks, ZIP integrity and runtime behaviour must be confirmed by the new GitHub Actions run and then tested on the target HP laptop. No claim is made that it is already tested on the device.
