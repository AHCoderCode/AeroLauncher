# Safety and Windows-preservation contract

AeroShell Desktop is a standalone WPF application window. It is not an operating-system installer, system-shell replacement, Windows optimizer, driver manager or disk utility.

## Protected Windows components

AeroShell and its build/release process must not:
- erase, format, repartition, reset, reinstall or replace Windows;
- modify Windows boot configuration, bootloader, Winlogon or the system shell;
- terminate or replace Explorer.exe as a shell-management action;
- modify Windows Defender, Firewall, Windows Update, security policy or services;
- install/remove drivers, flash firmware or change BIOS/UEFI settings;
- edit global registry settings, power plans or login startup configuration;
- move, rename, delete or change permissions on personal documents;
- create hidden services, scheduled tasks, auto-logon entries or persistence.

## Application behaviour

- Opens as one maximized, resizable WPF application window with standard Windows title-bar controls.
- Uses its own Start panel, taskbar-style dock, desktop shortcuts and navigation pages.
- Discovers local Start Menu shortcuts and launches them through the normal Windows shell.
- Opens Windows Explorer and native Settings pages when requested.
- Stores only AeroShell preferences in the current user's LocalAppData folder.
- Displays an optional wallpaper only inside the AeroShell window.
- Adds no automatic startup entry. Closing the app returns to the normal Windows desktop.
- Does not require administrator privileges.

## Release packaging

The portable release bundle contains the WPF executable and its build output, plus plain-text notes. It does not contain installer/uninstaller scripts, service installers, registry scripts, scheduled-task definitions or boot tools. No script should disable antivirus or request administrator privileges.

The app is currently unsigned. A source review, successful build, checksum or clean antivirus scan does not establish a malware-free guarantee. Review and scan the generated bundle. Do not bypass Defender or SmartScreen.

## Resource profile

There is no continuous animation loop or always-on helper process. The UI clock updates once per minute. Start Menu discovery runs after launch. Reduced-effects mode avoids loading the optional wallpaper. These measures are intended to reduce resource use; actual CPU, RAM, battery and GPU behaviour needs measurement on the target laptop.
