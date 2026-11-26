# AeroOS Windows Edition test plan

## Automated/source checks
- Verify WPF project/XAML/code-behind compile in Release mode.
- Verify release packaging excludes installer/uninstaller scripts and contains only portable app/runtime files and notes.
- Verify there are no boot, shell, Defender, service, scheduled-task, startup or global power-plan changes.
- Verify preferences are written only to the AeroOS LocalAppData directory.
- Verify Start Menu discovery is depth-limited and result-limited, handles inaccessible folders and skips reparse points.

## Windows runtime checks
- Launch manually as a standard user; confirm no administrator prompt is required.
- Confirm Exit/Alt+F4 reveals the normal Windows desktop and native taskbar.
- Confirm Explorer and the Windows shell continue running normally.
- Test Start, outside-click dismissal, Escape, Ctrl+Space, Ctrl+Comma and keyboard search/Enter.
- Search Start Menu shortcuts using exact names, partial names and no-match terms.
- Pin/unpin built-in and discovered apps; restart AeroOS and confirm pins persist.
- Launch several apps and confirm Recent updates and persists.
- Test built-in app launches, folder shortcuts and native Settings links.
- Test theme, accents, wallpaper choose/remove and Reduced Effects persistence.
- Test Start and Personalize at 800x600, 1024x768 and the laptop's native resolution/scaling.
- Confirm dashboard/header clocks and dates remain synchronized.
- Confirm hardware summary handles missing WMI fields.
- Measure idle CPU/RAM and battery discharge with AeroOS closed/open in both appearance modes.
- Scan the downloaded ZIP and executable with Windows Security before use.

## Status
CI can verify compilation and static source rules only. Interactive Windows runtime, hardware, display-scaling, security scan and power-impact tests must be completed on a real Windows device. A successful build is not a malware certification or a guarantee of zero bugs or a particular battery-life result.
