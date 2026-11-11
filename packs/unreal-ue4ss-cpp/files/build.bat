@echo off
rem Builds UE4SS and the mod in the configuration the released UE4SS uses (Game__Shipping__Win64).
setlocal
cd /d "%~dp0"

{{#if Ue4ss == xmake}}
if not exist "RE-UE4SS\xmake.lua" goto nosetup

xmake f -m "Game__Shipping__Win64" -y
if errorlevel 1 goto fail
xmake
if errorlevel 1 goto fail
{{#elif Ue4ss == stable}}
if not exist "RE-UE4SS\CMakeLists.txt" goto nosetup

cmake -S . -B Output -G "{{Generator}}"
if errorlevel 1 goto fail
cmake --build Output --config Game__Shipping__Win64 --target {{ProjectName}}
if errorlevel 1 goto fail
{{#else}}
if not exist "RE-UE4SS\CMakeLists.txt" goto nosetup

cmake -B build -G "{{Generator}}"
if errorlevel 1 goto fail
cmake --build build --config Game__Shipping__Win64 --target {{ProjectName}}
if errorlevel 1 goto fail
{{/if}}

echo.
echo Built. Now run install.bat.
goto end

:nosetup
echo RE-UE4SS is not here yet. Run setup.bat first.
goto end

:fail
echo.
echo Build failed. The first error above is the one that matters.

:end
echo.
pause
