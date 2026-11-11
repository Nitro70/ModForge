@echo off
rem Packs the mod folder into a pak and copies it into the game. See README.md.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack.ps1" %*
echo.
pause
