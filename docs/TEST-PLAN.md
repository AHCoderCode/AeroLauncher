# Test plan and current verification state

This is the initial implementation baseline. No test is marked as passed until it has actually been executed in the corresponding environment.

| Area | Test | Status |
|---|---|---|
| Build | Build `AeroOS.sln` in Release on Windows with .NET Framework 4.8 targeting pack | Not run in this Linux build workspace |
| Launch | Start `AeroOS.Windows.exe` on Windows 10 Pro x64 | Requires physical Windows test |
| Discovery | Enumerate user and common Start Menu shortcuts | Requires physical Windows test |
| Launching | Launch a harmless installed shortcut via ShellExecute | Requires physical Windows test |
| Search | Filter application names and categories | Code path implemented; runtime test pending |
| Preferences | Save/reload theme, accent, pins and history | Code path implemented; runtime test pending |
| Wallpaper | Display image inside AeroOS only; confirm Windows wallpaper unchanged | Requires physical Windows test |
| Control Center | Open supported Windows Settings URIs and fallback pages | Requires Windows-build-specific test |
| Diagnostics | Run read-only report in non-elevated Windows PowerShell 5.1 | Requires physical Windows test |
| Install | Copy app to current user's LocalAppData and create shortcuts | Requires physical Windows test |
| Uninstall | Remove only marked app folder and named shortcuts | Requires physical Windows test |
| Recovery | Confirm Explorer/taskbar remain functional after close/uninstall | Requires physical Windows test |
| Security | Confirm no startup persistence, services, drivers or boot changes | Static source review plus Windows test |
| Performance | Measure app idle working set and CPU on target laptop | Not measured; do not infer |

## Hardware performance baseline to collect

Before launching AeroOS, record Task Manager idle CPU, memory in use, disk activity, startup time, and launch times for File Explorer and the user's normal browser. Repeat after AeroOS is installed and idle. Use the same power mode and similar startup conditions. Report measured values and test conditions; do not attribute unrelated variation to AeroOS.
