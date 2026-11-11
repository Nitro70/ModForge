@echo off
rem Configures and builds the Release dll into build\Release.
rem Run it from a "Developer Command Prompt for VS" if cmake is not on PATH.
setlocal
cd /d "%~dp0"

{{#if Arch == x64}}
cmake -S . -B build -A x64
{{#else}}
cmake -S . -B build -A Win32
{{/if}}
if errorlevel 1 goto fail
cmake --build build --config Release
if errorlevel 1 goto fail

echo.
{{#if Mode == proxy}}
echo Built: build\Release\version.dll
{{#elif Mode == asi}}
echo Built: build\Release\{{ProjectName}}.asi
{{#else}}
echo Built: build\Release\{{ProjectName}}.dll
{{/if}}
goto end

:fail
echo.
echo Build failed. The first error above is the one that matters.
echo If the copy step failed, close the game: it keeps the file open while it runs.

:end
echo.
pause
