PRINTERUNLOCK - Windows 10 x64 diagnostic prototype
===================================================

BUILD WITH GITHUB ACTIONS (NO LOCAL COMPILER NEEDED)
1. Create a GitHub repository.
2. Upload all files and folders in this project, including the hidden
   .github/workflows/build.yml file.
3. Commit/push to the main or master branch.
4. Open the repository's Actions tab.
5. Select "Build PrinterUnlock (Windows 10 x64)".
6. Wait for the green successful run.
7. Open the run and download the artifact named
   PrinterUnlock-Windows10-x64.
8. Extract it; it contains PrinterUnlock.exe.

You can also start a build manually from Actions > Build PrinterUnlock
> Run workflow.

LOCAL BUILD (OPTIONAL)
- Windows 10 x64
- Visual Studio / Build Tools with MSBuild
- .NET Framework 4.8 targeting pack
- Run BUILD.bat

WHAT THIS VERSION DOES
- Lists Windows printer queues.
- Shows driver name, port, status and PnP device ID.
- Searches Plug-and-Play devices for Canon / USB printer entries.
- Exports a diagnostic text report.

WHAT IT DOES NOT DO
- It does not flash, patch, unlock or rewrite printer firmware.
- It does not send undocumented USB commands.
- It cannot defeat a cartridge-presence check enforced by the printer itself.
- It does not claim to support every MG25xxx model's internal protocol.

SAFETY
Do not flash firmware from another model or region. Do not send guessed
vendor-specific USB commands. Keep original firmware and record the exact
model suffix and firmware version before any firmware research.

This is a source-code prototype. The GitHub workflow builds the executable
on a hosted Windows runner; it does not test it against a physical printer.
