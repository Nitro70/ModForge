# Copies the built dll into the game as Mods\{{ProjectName}}\dlls\main.dll.
#   install.bat                     uses the folder written in game-path.txt
#   install.bat "D:\Games\MyGame"   uses that folder instead
# The folder can be the game's top folder or the folder that holds UE4SS.dll.
param([string]$Game = "")

$ErrorActionPreference = 'Stop'
$ModName = '{{ProjectName}}'
{{#if EnabledTxt}}
$UseEnabledTxt = $true
{{#else}}
$UseEnabledTxt = $false
{{/if}}
$Here = Split-Path -Parent $MyInvocation.MyCommand.Path

# The newest {{ProjectName}}.dll under the build folders (CMake: build or Output, xmake: Binaries).
$Dll = $null
foreach ($dir in @('build', 'Output', 'Binaries')) {
    $full = Join-Path $Here $dir
    if (-not (Test-Path -LiteralPath $full)) { continue }
    foreach ($file in Get-ChildItem -LiteralPath $full -Recurse -File -Filter "$ModName.dll") {
        if ($null -eq $Dll -or $file.LastWriteTime -gt $Dll.LastWriteTime) { $Dll = $file }
    }
}
if ($null -eq $Dll) {
    Write-Host "No $ModName.dll found. Run build.bat first."
    exit 1
}
Write-Host "Built dll: $($Dll.FullName)"
if ($Dll.FullName -notmatch 'Shipping') {
    Write-Host "Note: this dll is not from a Shipping configuration. It only loads in a UE4SS built the same way."
}

if (-not $Game) {
    $pathFile = Join-Path $Here 'game-path.txt'
    if (Test-Path -LiteralPath $pathFile) {
        $Game = [IO.File]::ReadAllText($pathFile, [Text.Encoding]::UTF8).Trim()
    }
}
if (-not $Game) {
    Write-Host "No game folder. Put the game's folder on the first line of game-path.txt, or run: install.bat ""D:\Games\MyGame"""
    exit 1
}
if (-not (Test-Path -LiteralPath $Game)) {
    Write-Host "This folder does not exist: $Game"
    exit 1
}

# UE4SS keeps its mods in Mods\ next to UE4SS.dll, or in ue4ss\Mods\ on newer installs.
# Both live in <Game>\<Project>\Binaries\Win64 (WinGDK for the Microsoft Store version).
function Find-ModsFolder([string]$root) {
    $candidates = @($root)
    foreach ($dir in Get-ChildItem -LiteralPath $root -Directory) {
        $candidates += (Join-Path $dir.FullName 'Binaries\Win64')
        $candidates += (Join-Path $dir.FullName 'Binaries\WinGDK')
    }
    foreach ($candidate in $candidates) {
        foreach ($sub in @('ue4ss\Mods', 'Mods')) {
            $mods = Join-Path $candidate $sub
            if (Test-Path -LiteralPath (Join-Path $mods 'mods.txt')) { return $mods }
        }
    }
    return $null
}

$Mods = Find-ModsFolder $Game
if (-not $Mods) {
    Write-Host "No UE4SS Mods folder (one with a mods.txt in it) under: $Game"
    Write-Host "Install UE4SS into the game first, start the game once, then run this again."
    exit 1
}

$Target = Join-Path $Mods $ModName
$TargetDlls = Join-Path $Target 'dlls'
New-Item -ItemType Directory -Force -Path $TargetDlls | Out-Null
try {
    Copy-Item -LiteralPath $Dll.FullName -Destination (Join-Path $TargetDlls 'main.dll') -Force
}
catch {
    Write-Host "Could not write main.dll: $($_.Exception.Message)"
    Write-Host "Close the game first: it keeps the dll open while it runs."
    exit 1
}

$ModsTxt = Join-Path $Mods 'mods.txt'
$bytes = [IO.File]::ReadAllBytes($ModsTxt)
$hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
if ($hasBom) {
    Write-Host "Note: mods.txt starts with a byte-order mark. UE4SS 3.0.1 then skips the mod on its first line."
}

$TargetEnabledTxt = Join-Path $Target 'enabled.txt'
if ($UseEnabledTxt) {
    # An empty enabled.txt in the mod folder is enough: UE4SS starts the mod after the ones listed in mods.txt.
    [IO.File]::WriteAllBytes($TargetEnabledTxt, (New-Object byte[] 0))
    Write-Host "Enabled by enabled.txt."
}
else {
    # No enabled.txt: the mod needs a line in mods.txt, above the built-in Keybinds entry.
    if (Test-Path -LiteralPath $TargetEnabledTxt) { Remove-Item -LiteralPath $TargetEnabledTxt -Force }
    $text = [IO.File]::ReadAllText($ModsTxt)
    $newline = "`n"
    if ($text.Contains("`r`n")) { $newline = "`r`n" }
    $lines = New-Object System.Collections.Generic.List[string]
    foreach ($line in ($text -split "`r?`n")) { $lines.Add($line) }

    $entry = '^\s*' + [regex]::Escape($ModName) + '\s*:'
    $index = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match $entry) { $index = $i; break }
    }
    if ($index -ge 0) {
        $lines[$index] = "$ModName : 1"
    }
    else {
        # Insert after the last mod line that comes before the Keybinds block.
        $at = $lines.Count
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match '^\s*;.*[Kk]eybinds' -or $lines[$i] -match '^\s*Keybinds\s*:') { $at = $i; break }
        }
        while ($at -gt 0 -and $lines[$at - 1].Trim().Length -eq 0) { $at-- }
        $lines.Insert($at, "$ModName : 1")
    }
    [IO.File]::WriteAllText($ModsTxt, ($lines -join $newline), (New-Object System.Text.UTF8Encoding($hasBom)))
    Write-Host "Enabled in mods.txt: $ModName : 1"
}

Write-Host "Installed to: $(Join-Path $TargetDlls 'main.dll')"
Write-Host "Start the game, then look in UE4SS.log for: [$ModName] loaded"
