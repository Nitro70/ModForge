# Packs mod\ into dist\{{ModId}}.zip, the way BeamNG wants it:
#   - the zip root IS the mod root (lua\, scripts\, ui\, vehicles\ sit at the top, no wrapping folder)
#   - every entry name uses forward slashes
# Compress-Archive in Windows PowerShell 5.1 writes backslashes, which is why this script
# builds the zip entry by entry instead.
#
#   powershell -ExecutionPolicy Bypass -File pack.ps1            build only
#   powershell -ExecutionPolicy Bypass -File pack.ps1 -Install   build, then copy into the mods folder
param(
    [switch]$Install
)

$ErrorActionPreference = 'Stop'

$modName = '{{ModId}}'
$root = $PSScriptRoot
$modDir = Join-Path $root 'mod'
$distDir = Join-Path $root 'dist'
$zipPath = Join-Path $distDir ($modName + '.zip')

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not (Test-Path -LiteralPath $modDir)) {
    throw "mod folder not found: $modDir"
}

$files = @(Get-ChildItem -LiteralPath $modDir -Recurse -File | Sort-Object FullName)
if ($files.Count -eq 0) {
    throw "nothing to pack: $modDir is empty"
}

New-Item -ItemType Directory -Force -Path $distDir | Out-Null
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath
}

$prefixLength = $modDir.TrimEnd('\').Length + 1
$archive = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in $files) {
        $entryName = $file.FullName.Substring($prefixLength).Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive, $file.FullName, $entryName,
            [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $archive.Dispose()
}

# Read the zip back: a bad entry name is the classic reason a mod is mounted but does nothing.
$check = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $names = @($check.Entries | ForEach-Object { $_.FullName })
}
finally {
    $check.Dispose()
}
$bad = @($names | Where-Object { $_.Contains('\') })
if ($bad.Count -gt 0) {
    throw "zip entries with backslashes: $($bad -join ', ')"
}
$topLevel = @($names | ForEach-Object { $_.Split('/')[0] } | Sort-Object -Unique)
$known = @('lua', 'scripts', 'ui', 'vehicles', 'levels', 'art', 'settings', 'gameplay', 'mod_info')
$unknown = @($topLevel | Where-Object { $known -notcontains $_ })
if ($unknown.Count -gt 0) {
    Write-Warning "unusual top-level entries (the game may ignore them): $($unknown -join ', ')"
}

Write-Host "built $zipPath ($($names.Count) files)"
$names | ForEach-Object { Write-Host "  $_" }

if ($Install) {
    # Writing over a zip the running game has mounted breaks the mod until the game restarts.
    $running = @(Get-Process | Where-Object { $_.ProcessName -like 'BeamNG*' })
    if ($running.Count -gt 0) {
        throw "BeamNG is running. Close it first, then run this again."
    }
    $modsDir = Join-Path $env:LOCALAPPDATA 'BeamNG\BeamNG.drive\current\mods'
    if (-not (Test-Path -LiteralPath $modsDir)) {
        throw "mods folder not found: $modsDir (start the game once, or copy the zip by hand: Launcher > Manage User Folder)"
    }
    $target = Join-Path $modsDir ($modName + '.zip')
    Copy-Item -LiteralPath $zipPath -Destination $target -Force
    Write-Host "installed $target"
}
