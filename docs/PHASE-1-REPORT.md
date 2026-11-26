# AeroOS Windows Edition 0.4.0 implementation and QA report

## Implemented in source

The 0.4.0 pass focuses on everyday usability while preserving the existing Windows installation:
- Persistent app pins and recent-app history.
- Ranked Start search and Enter-to-launch.
- Keyboard shortcuts for Start and Personalize.
- Synchronized dashboard/header clock and date.
- Responsive desktop columns and screen-aware overlay dimensions.
- Scrollable Personalize panel.
- Expanded best-effort hardware summary.
- Bounded Start Menu shortcut discovery with inaccessible-folder handling and reparse-point avoidance.

## Windows-preservation boundaries

The app remains a normal, manually launched WPF window. It does not replace Explorer, change the system shell, modify the bootloader, edit global registry or power settings, create services/scheduled tasks/startup entries, change Defender, install drivers or touch disk partitions. Preferences remain under the current user's LocalAppData AeroOS folder.

## Verification status

The previous 0.3.0 source passed Windows Release compilation and static checks. The new 0.4.0 source must pass the workflow after these changes. Even if it succeeds, interactive testing on the HP ProBook 4520s remains outstanding, including screen scaling/layout, app launch behaviour, preference persistence, Windows Security scan and measured CPU/RAM/battery use. No malware-free or zero-bug guarantee is made.
