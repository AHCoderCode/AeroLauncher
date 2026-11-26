# Portable application bundle

AeroOS Windows Edition 0.4.0 is distributed as a portable ZIP, not an installer.

The GitHub Actions workflow builds the native WPF application on a Windows runner and uploads a versioned ZIP and SHA-256 file. Extract the ZIP to a normal user folder. The bundle contains the compiled application, its .NET configuration/runtime files and plain-text notes. It does not install a service, create a startup entry, replace the Windows shell or change boot configuration.

The application targets Windows 10 x64 and .NET Framework 4.8. It is unsigned. A successful compile, static source checks and a matching checksum do not establish that a file is malware-free. Scan the downloaded ZIP and extracted executable with Windows Security; any earlier antivirus detections must also be reviewed before launch. Do not bypass a Defender or SmartScreen warning.

AeroOS does not require administrator privileges. If a UAC prompt appears, cancel it and do not proceed. The app opens as one maximized window and can be closed with its Exit button or Alt+F4 to reveal the normal Windows desktop.
