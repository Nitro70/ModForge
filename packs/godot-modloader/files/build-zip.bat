@echo off
rem Builds the mod zip into dist\ and copies it into the game's mods folder. See README.md.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-zip.ps1" %*
echo.
pause
