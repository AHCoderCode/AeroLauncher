# Windows support and hardware compatibility notes

Checked against Microsoft documentation on 2026-11-25.

## Windows 10 status

Windows 10 reached end of normal support on October 14, 2025. Eligible Windows 10 22H2 devices can enroll in the consumer Extended Security Updates (ESU) program, which provides critical and important security updates through October 12, 2027. ESU does not restore feature updates or general technical support.

Microsoft's current consumer ESU prerequisites include Windows 10 22H2 Home, Professional, Pro Education or Workstations; current Windows updates; the ESU Licensing Preparation Package (KB5126256); and an administrator Microsoft account that is not a child account. Availability and enrollment can vary by market. Check the eligibility link in **Settings > Update & Security > Windows Update** with the account holder/parent or guardian where applicable. Do not bypass account or family restrictions.

Official references:

- https://www.microsoft.com/en-us/windows/end-of-support
- https://www.microsoft.com/en-us/windows/extended-security-updates

## Windows 11 CPU eligibility

The Linux AeroOS repository README documents the laptop target as an Intel Core i5-480M. Microsoft's supported Intel processor list starts with much newer Core generations (for example, 8th-generation Core i3/i5/i7/i9 families, plus separately listed families). The i5-480M is not listed. If the Windows diagnostic report confirms that exact CPU, the laptop does not meet Microsoft's supported Intel CPU list for Windows 11. Other Windows 11 requirements (UEFI/Secure Boot, TPM, memory, storage, graphics and display) also need checking, but a failed CPU requirement is already sufficient to treat it as unsupported.

Do not use bypasses or install an unsupported Windows 11 image as part of AeroOS. AeroOS does not modify Windows setup, boot configuration or firmware.

Official reference:

- https://learn.microsoft.com/en-us/windows-hardware/design/minimum/supported/windows-11-supported-intel-processors

## What AeroOS does about this

AeroOS remains a normal desktop app for the user's existing Windows installation. It neither upgrades Windows nor changes Windows Update/ESU state. The diagnostic script reports the current Windows build and hardware so the owner can verify the actual conditions first.
