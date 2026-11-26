# AeroOS Windows Edition test plan

## Static/source checks
- Verify the WPF project and all XAML/code-behind files are included in the solution.
- Verify the release packaging excludes installer/uninstaller scripts.
- Verify there are no boot, shell, Defender, service, scheduled-task or global power-plan changes.
- Verify preferences are written only to the AeroOS LocalAppData directory.

## Windows build checks
- Build Release configuration using MSBuild on a Windows runner.
- Confirm the executable exists and that the portable ZIP and SHA-256 file are generated.
- Confirm the ZIP contains application files and plain-text notes only.

## Runtime checks on a test Windows 10 device
- Launch the app manually; confirm the full-screen window displays and Alt+F4/Exit closes it.
- Confirm Windows Explorer and the native taskbar/desktop are unchanged after exit.
- Test Start Menu discovery/search and each built-in app link.
- Test all folder shortcuts and native Settings links.
- Test theme, accent colour, wallpaper and Reduced Effects preferences persist correctly.
- Confirm app discovery and WMI run once, and the clock timer refreshes once per minute.
- Measure idle CPU/RAM and battery discharge with AeroOS closed and open, in both visual modes.
- Inspect Windows Security scan results and application behaviour before daily use.

## Status
The above runtime and physical-device checks have not been completed in this environment.
