@echo off
setlocal
echo Building PrinterUnlock for Windows 10 x64...
where msbuild >nul 2>nul
if errorlevel 1 (
  echo MSBuild was not found.
  echo Install Visual Studio Build Tools with the .NET desktop build tools workload,
  echo or open PrinterUnlock.csproj in Visual Studio.
  pause
  exit /b 1
)
msbuild PrinterUnlock.csproj /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if errorlevel 1 (
  echo Build failed. Check the messages above.
  pause
  exit /b 1
)
echo.
echo Build complete. Look in bin\Release\PrinterUnlock.exe
pause
