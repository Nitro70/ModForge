@echo off
rem Makes this folder a git repo and adds RE-UE4SS as a submodule at the version the mod is built against.
rem Needs git, and a GitHub account that is linked to an Epic Games account and has an SSH key:
rem UE4SS pulls Unreal's headers (the UEPseudo repo) from a repo that only linked accounts can read.
setlocal
cd /d "%~dp0"

{{#if Ue4ssRef}}
set "REF={{Ue4ssRef}}"
{{#elif Ue4ss == stable}}
set "REF=v3.0.1"
{{#elif Ue4ss == xmake}}
set "REF=experimental-latest"
{{#else}}
set "REF=experimental2"
{{/if}}

where git >nul 2>nul
if errorlevel 1 goto nogit

if exist ".git" goto haverepo
git init
if errorlevel 1 goto fail
:haverepo

if exist "RE-UE4SS\.git" goto havesub
git submodule add https://github.com/UE4SS-RE/RE-UE4SS.git RE-UE4SS
if errorlevel 1 goto fail
:havesub

cd RE-UE4SS
git fetch --tags
if errorlevel 1 goto fail
git checkout %REF%
if errorlevel 1 goto fail
git submodule update --init --recursive
if errorlevel 1 goto fail
cd ..

echo.
echo RE-UE4SS is ready at %REF%. Now run build.bat.
goto end

:nogit
echo git was not found. Install Git for Windows, then run this again.
goto end

:fail
echo.
echo Setup failed. If the error mentions UEPseudo or "Permission denied (publickey)":
echo link your GitHub account to your Epic Games account, accept the EpicGames organisation invite,
echo and add an SSH key to your GitHub account. Then run this again.

:end
echo.
pause
