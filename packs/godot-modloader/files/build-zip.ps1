# Builds dist\{{ModId}}-<version>.zip and copies it into the game's mods folder.
#   build-zip.bat                     builds, then installs into the game from game-path.txt
#   build-zip.bat "D:\Games\MyGame"   installs into that game folder instead
#   build-zip.bat -NoInstall          builds only
param([string]$Game = "", [switch]$NoInstall)

$ErrorActionPreference = 'Stop'
$ModId = '{{ModId}}'
$Here = Split-Path -Parent $MyInvocation.MyCommand.Path
$ModDir = Join-Path (Join-Path $Here 'mods-unpacked') $ModId

# ---- check what the Mod Loader checks, so a mistake shows up here and not as a mod that is not there ----

$manifestPath = Join-Path $ModDir 'manifest.json'
if (-not (Test-Path -LiteralPath $manifestPath)) {
    Write-Host "Missing: $manifestPath"
    Write-Host "The folder under mods-unpacked must be called $ModId (namespace-name from manifest.json)."
    exit 1
}
if (-not (Test-Path -LiteralPath (Join-Path $ModDir 'mod_main.gd'))) {
    Write-Host "Missing: $(Join-Path $ModDir 'mod_main.gd')"
    exit 1
}
$manifest = [IO.File]::ReadAllText($manifestPath, [Text.Encoding]::UTF8) | ConvertFrom-Json
$problems = @()
foreach ($key in @('name', 'namespace', 'version_number', 'website_url', 'description', 'dependencies', 'extra')) {
    if ($null -eq $manifest.PSObject.Properties[$key]) { $problems += "manifest.json has no ""$key""" }
}
if ($problems.Count -eq 0) {
    $godot = $null
    if ($null -ne $manifest.extra.PSObject.Properties['godot']) { $godot = $manifest.extra.godot }
    if ($null -eq $godot) {
        $problems += 'manifest.json has no "extra.godot"'
    }
    else {
        foreach ($key in @('authors', 'compatible_mod_loader_version', 'compatible_game_version')) {
            if ($null -eq $godot.PSObject.Properties[$key]) { $problems += "manifest.json has no ""extra.godot.$key""" }
        }
    }
    foreach ($value in @($manifest.name, $manifest.namespace)) {
        if ($value -notmatch '^[a-zA-Z0-9_]{3,}$') { $problems += """$value"": name and namespace take letters, digits and _ only, 3 characters or more" }
    }
    if ($manifest.version_number -notmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
        $problems += """$($manifest.version_number)"": version_number must be three numbers like 1.0.0"
    }
    if ("$($manifest.namespace)-$($manifest.name)" -cne $ModId) {
        $problems += "manifest.json says the mod is ""$($manifest.namespace)-$($manifest.name)"" but its folder is ""$ModId"". Rename the folder (and `$ModId at the top of this script) or fix the manifest."
    }
}
if ($problems.Count -gt 0) {
    foreach ($problem in $problems) { Write-Host "Problem: $problem" }
    exit 1
}

# ---- the zip: mods-unpacked/<mod id>/... with forward slashes, which is what Godot reads ----

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$Dist = Join-Path $Here 'dist'
New-Item -ItemType Directory -Force -Path $Dist | Out-Null
$zipPath = Join-Path $Dist "$ModId-$($manifest.version_number).zip"
if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }

$files = @(Get-ChildItem -LiteralPath $ModDir -Recurse -File)
$stream = [IO.File]::Open($zipPath, [IO.FileMode]::CreateNew)
$zip = New-Object System.IO.Compression.ZipArchive($stream, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($ModDir.Length).TrimStart('\').Replace('\', '/')
        $entry = $zip.CreateEntry("mods-unpacked/$ModId/$relative", [System.IO.Compression.CompressionLevel]::Optimal)
        $target = $entry.Open()
        try {
            $bytes = [IO.File]::ReadAllBytes($file.FullName)
            $target.Write($bytes, 0, $bytes.Length)
        }
        finally { $target.Dispose() }
    }
}
finally {
    $zip.Dispose()
    $stream.Dispose()
}
Write-Host "Built: $zipPath ($($files.Count) files)"
if ($NoInstall) { exit 0 }

# ---- install: the zip goes into the mods folder next to the game's exe ----

if (-not $Game) {
    $pathFile = Join-Path $Here 'game-path.txt'
    if (Test-Path -LiteralPath $pathFile) {
        $Game = [IO.File]::ReadAllText($pathFile, [Text.Encoding]::UTF8).Trim()
    }
}
if (-not $Game) {
    Write-Host "Not installed: no game folder. Put it on the first line of game-path.txt, or pass it to build-zip.bat."
    exit 0
}
if (-not (Test-Path -LiteralPath $Game)) {
    Write-Host "Not installed: this folder does not exist: $Game"
    exit 1
}
function Test-GameFolder([string]$dir) {
    return @(Get-ChildItem -LiteralPath $dir -File | Where-Object { $_.Extension -eq '.exe' -or $_.Extension -eq '.pck' }).Count -gt 0
}
$exeDir = $Game
if (-not (Test-GameFolder $Game)) {
    # Some games keep the exe one folder down.
    $inside = @(Get-ChildItem -LiteralPath $Game -Directory | Where-Object { Test-GameFolder $_.FullName })
    if ($inside.Count -ne 1) {
        Write-Host "Not installed: no game exe in $Game"
        exit 1
    }
    $exeDir = $inside[0].FullName
}
$modsDir = Join-Path $exeDir 'mods'
New-Item -ItemType Directory -Force -Path $modsDir | Out-Null
# Older versions of this mod would load next to the new one: remove them.
Get-ChildItem -LiteralPath $modsDir -File -Filter "$ModId-*.zip" | Remove-Item -Force
Copy-Item -LiteralPath $zipPath -Destination $modsDir -Force
Write-Host "Installed to: $modsDir"
