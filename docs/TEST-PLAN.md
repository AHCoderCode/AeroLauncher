# AeroShell Desktop test plan

## Automated/source checks
- Verify WPF project/XAML/code-behind compile in Release mode.
- Verify release packaging excludes installer/uninstaller scripts and contains only portable app/runtime files and notes.
- Verify there are no boot, shell, Defender, service, scheduled-task, startup or global power-plan changes.
- Verify preferences are written only to the AeroShell LocalAppData directory.
- Verify Start Menu discovery is depth-limited and result-limited, handles inaccessible folders and skips reparse points.
- Verify the workflow uploads an artifact named AeroShell-Windows-v0.7.0 with its SHA-256 file.

## Windows runtime checks
- Confirm the window starts maximized and native minimize, maximize/restore, resize and close controls behave normally.
- Confirm Desktop shows one-click shortcuts and opens only the selected existing Windows app or folder.
- Confirm Desktop, All apps, Files & folders and Personalization can be reached from both sidebar and taskbar.
- Confirm Personalization changes appearance without changing the user's normal Windows wallpaper or global settings.
- Confirm Exit/Alt+F4 reveals the normal Windows desktop and native taskbar.
- Confirm Explorer and the Windows shell continue running normally.
- Test Start outside-click dismissal, Escape, Ctrl+Space and app search/Enter.
- Search Start Menu shortcuts using exact names, partial names and no-match terms.
- Pin/unpin built-in and discovered apps; restart AeroShell and confirm pins persist.
- Launch several apps and confirm Recent updates and persists.
- Test built-in app launches, folder shortcuts and native Settings links.
- Test theme, accents, wallpaper choose/remove and Reduced Effects persistence.
- Test Personalization at 880x560, 1024x768 and the laptop's native resolution/scaling.
- Confirm the workspace clock/date refresh correctly.
- Confirm missing Start Menu shortcuts and unavailable apps are handled without crashing.
- Measure idle CPU/RAM and battery discharge with AeroShell closed/open in both appearance modes.
- Scan the downloaded ZIP and executable with Windows Security before use.

## Status
CI can verify compilation, packaging and static source rules only. Interactive Windows runtime, hardware, display-scaling, security scan and power-impact tests must be completed on a real Windows device. A successful build is not a malware certification or a guarantee of zero bugs or a particular battery-life result.
