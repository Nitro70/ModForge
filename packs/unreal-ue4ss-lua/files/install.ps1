# Copies this mod into the game's UE4SS Mods folder.
#   install.bat                     uses the folder written in game-path.txt
#   install.bat "D:\Games\MyGame"   uses that folder instead
# The folder can be the game's top folder or the folder that holds UE4SS.dll.
param([string]$Game = "")

$ErrorActionPreference = 'Stop'
$ModName = '{{ProjectName}}'
$Here = Split-Path -Parent $MyInvocation.MyCommand.Path

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
$TargetScripts = Join-Path $Target 'Scripts'
if (Test-Path -LiteralPath $TargetScripts) { Remove-Item -LiteralPath $TargetScripts -Recurse -Force }
New-Item -ItemType Directory -Force -Path $Target | Out-Null
Copy-Item -LiteralPath (Join-Path $Here 'Scripts') -Destination $Target -Recurse -Force

$ModsTxt = Join-Path $Mods 'mods.txt'
$bytes = [IO.File]::ReadAllBytes($ModsTxt)
$hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
if ($hasBom) {
    Write-Host "Note: mods.txt starts with a byte-order mark. UE4SS 3.0.1 then skips the mod on its first line."
}

$EnabledTxt = Join-Path $Here 'enabled.txt'
$TargetEnabledTxt = Join-Path $Target 'enabled.txt'
if (Test-Path -LiteralPath $EnabledTxt) {
    # enabled.txt in the mod folder is enough: UE4SS starts the mod after the ones listed in mods.txt.
    Copy-Item -LiteralPath $EnabledTxt -Destination $TargetEnabledTxt -Force
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

Write-Host "Installed to: $Target"
Write-Host "Start the game, then look in UE4SS.log for: Starting Lua mod '$ModName'"
