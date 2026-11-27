# Changelog

## 0.7.0 - 2026-11-27

- Reworked the Home destination into a desktop-style shortcut surface with one-click shortcuts for common Windows apps, folders, This PC and Recycle Bin.
- Added a dedicated Personalization navigation page instead of opening settings as a blocking overlay.
- Added Desktop, Apps, Files and Personalization navigation to the centered taskbar.
- Restored normal Windows minimize, maximize/restore, resize and close controls while keeping AeroShell maximized on launch.
- Fixed the duplicate clock-label assignment so the visible workspace clock refreshes correctly.
- Removed the unused one-time WMI hardware poll from startup to avoid unnecessary work on older laptops.
- Kept wallpaper, theme and preferences local to AeroShell and preserved the no-installer/no-shell-takeover/no-boot-change safety contract.
- Updated the portable package instructions and release artifact name to 0.7.0.
- Build and static checks are automated; real laptop UI, scaling and performance checks remain pending.


## 0.4.0 - 2026-11-26

- Added persistent custom app pinning (up to 12 entries) and an eight-entry recent-app history.
- Added priority-ranked Start search and Enter-to-launch behaviour.
- Added Ctrl+Space for Start, Ctrl+Comma for Personalize, and Escape for closing overlays.
- Fixed the dashboard's large clock/date so both update every minute.
- Added compact desktop layout behaviour for narrower displays and screen-aware Start/Personalize panel sizing.
- Made Personalize content scrollable rather than relying on a fixed-height card.
- Expanded the one-time device summary with graphics, available RAM and system-drive capacity.
- Bounded Start Menu shortcut discovery by depth and total result count, and skipped reparse-point folders.
- Preserved the no-installer, no-startup, no-services, no-shell-takeover, no-boot-change and no-Defender-change safety boundaries.

## 0.3.0 - 2026-11-26

- Upgraded the visual system with layered glass gradients, translucent panels, brighter edge highlights and static ambient lighting.
- Added rounded, consistent button and text-input templates with hover, press and focus feedback.
- Preserved the static rendering approach: no continuous animation loops or expensive blur effects were added.
- Windows build and HP ProBook runtime/layout/performance verification are required.

## 0.2.0 - 2026-11-26

- Rebuilt the main experience as a custom, full-screen, glass-style WPF launcher window.
- Added an in-app Start panel, shortcut search, pinned Windows tools and an in-app taskbar dock.
- Added shortcuts to common user folders, This PC and Recycle Bin.
- Added Windows Settings quick links, clock/date and one-time device summary.
- Added AeroOS-only theme, accent, wallpaper and Reduced Effects preferences.
- Replaced the previous script-based installer bundle with a portable application ZIP.
- Removed the old 0.1.0 source archive and user-level installer/uninstaller scripts from the release path.
- Kept Windows shell, boot, Defender, services, startup and power settings outside the app's scope.

## 0.1.0

- Initial native WPF desktop companion prototype.
- Start Menu shortcut discovery, basic personalization and system information.
