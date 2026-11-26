# Portable app bundle

AeroOS Windows Edition 0.2.0 is distributed as a portable application bundle rather than a script-based installer.

The GitHub Actions build creates a ZIP containing the compiled WPF application, runtime files produced by MSBuild and plain-text notes. It does not include PowerShell install/uninstall scripts.

The application targets Windows 10 and .NET Framework 4.8. It is unsigned. A successful compile does not replace runtime testing or security review. Do not treat static source checks as proof that an executable is malware-free.

AeroOS does not require elevation, create startup persistence or replace Windows Explorer. It runs as a maximized window and can be closed to reveal the native Windows desktop.
