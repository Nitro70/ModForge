@echo off
rem Copies the built dll into the game's UE4SS Mods folder. Pass a game folder to override game-path.txt.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" %*
echo.
pause
