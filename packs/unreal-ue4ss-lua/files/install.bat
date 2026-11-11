@echo off
rem Copies this mod into the game's UE4SS Mods folder. Pass a game folder to override game-path.txt.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" %*
echo.
pause
