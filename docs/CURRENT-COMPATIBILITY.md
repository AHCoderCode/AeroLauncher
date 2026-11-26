# Current compatibility notes

- Target operating system: Windows 10, 64-bit.
- UI framework: WPF on .NET Framework 4.8.
- Intended hardware class: older dual-core laptops with integrated or entry-level graphics and 8 GB RAM.
- The HP ProBook 4520s exact CPU/GPU and Windows build should be confirmed using a trusted local diagnostic.
- Windows Explorer, browser, Settings, Calculator, Paint, Notepad and Task Manager are launched from the existing Windows installation.
- Start Menu discovery depends on local shortcut files; some Microsoft Store applications may not appear in the app list.
- The app uses a one-time WMI hardware query; hardware fields may be unavailable on some systems.
- Glass-style panels are WPF translucent surfaces, not guaranteed native Acrylic blur on every Windows 10 build.
- Reduced Effects mode removes the optional background image. It does not change Windows power settings or system services.
- Native ms-settings page support can vary by Windows version; Windows Settings remains available from the user's normal desktop.
