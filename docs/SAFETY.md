# Safety and Windows-preservation contract

AeroOS Windows Edition is a standalone WPF application window. It is not an operating-system installer, system-shell replacement, Windows optimizer, driver manager or disk utility. It is separate from the AeroShell Desktop project.

## Protected Windows components

AeroOS and its build/release process must not:
- erase, format, repartition, reset, reinstall or replace Windows;
- modify Windows boot configuration, bootloader, Winlogon or the system shell;
- terminate or replace Explorer.exe as a shell-management action;
- modify Windows Defender, Firewall, Windows Update, security policy or services;
- install/remove drivers, flash firmware or change BIOS/UEFI settings;
- edit global registry settings, power plans or login startup configuration;
- move, rename, delete or change permissions on personal documents;
- create hidden services, scheduled tasks, auto-logon entries or persistence.

## Application behaviour

- Opens as one maximized, borderless application window.
- Uses its own in-app Start panel, taskbar-like dock, search, tiles and side links.
- Discovers local Start Menu shortcuts and launches them through the normal Windows shell.
- Opens Windows Explorer and native Settings pages when requested.
- Reads a small amount of hardware information once after launch using WMI.
- Stores only AeroOS preferences in the current user's LocalAppData folder.
- Displays optional wallpaper only inside its own window.
- Adds no automatic startup entry. Closing the app returns to the normal Windows desktop.

## Release packaging

The portable release bundle contains the WPF executable and its build output, plus plain-text project notes. It does not contain installer/uninstaller scripts, service installers, registry scripts, batch launchers, scheduled-task definitions or boot tools. No script should disable antivirus or request administrator privileges.

The app is currently unsigned. A source review and static checks do not establish a malware-free guarantee. Review and scan the generated bundle, and validate it on a test environment before daily use.

## Resource profile

There is no continuous animation loop or always-on helper process. A UI clock updates once per minute. App discovery and the hardware summary run once after launch. Reduced-effects mode disables the optional background image. These measures are intended to reduce resource use; actual CPU, RAM, battery and GPU behaviour needs measurement on the target laptop.
