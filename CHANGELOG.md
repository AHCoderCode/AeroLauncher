# Changelog

## 0.3.0 - 2026-11-26

- Upgraded the visual system with layered glass gradients, translucent panels, brighter edge highlights and static ambient lighting.
- Added rounded, consistent button and text-input templates with hover, press and focus feedback.
- Preserved the static rendering approach: no continuous animation loops or expensive blur effects were added.
- Kept all application behaviour and Windows-preservation boundaries unchanged.
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
- Windows build and target-device runtime/performance tests are still required.

## 0.1.0

- Initial native WPF desktop companion prototype.
- Start Menu shortcut discovery, basic personalization and system information.
