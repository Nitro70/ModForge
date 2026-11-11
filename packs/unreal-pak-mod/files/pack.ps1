# Packs the {{PakName}} folder into a mod pak and copies it into the game.
#   pack.bat                     packs, then installs into the game from game-path.txt
#   pack.bat "D:\Games\MyGame"   installs into that game folder instead
#   pack.bat -NoInstall          packs only; the result stays in dist\
param([string]$Game = "", [switch]$NoInstall)

$ErrorActionPreference = 'Stop'
$PakName = '{{PakName}}'
$Project = '{{UnrealProject}}'
$EngineVersion = '{{EngineVersion}}'
$ModsFolder = '{{ModsFolder}}'
{{#if Format == iostore}}
$IoStore = $true
{{#else}}
$IoStore = $false
{{/if}}
$Placeholder = 'put-cooked-files-here.txt'

$Here = Split-Path -Parent $MyInvocation.MyCommand.Path
$Stage = Join-Path $Here $PakName
$Dist = Join-Path $Here 'dist'

function Find-Tool([string]$name, [string]$envName) {
    $fromEnv = [Environment]::GetEnvironmentVariable($envName)
    if ($fromEnv -and (Test-Path -LiteralPath $fromEnv)) { return $fromEnv }
    $local = Join-Path (Join-Path $Here 'tools') "$name.exe"
    if (Test-Path -LiteralPath $local) { return $local }
    $onPath = Get-Command "$name.exe" -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }
    return $null
}

# The pak format version repak must write for an engine version (the table in repak's README).
function Get-RepakVersion([string]$engine) {
    $parts = $engine.Split('.')
    $major = [int]$parts[0]
    $minor = 0
    if ($parts.Length -gt 1) { $minor = [int]$parts[1] }
    if ($major -ge 5) { return 'V11' }
    if ($minor -ge 26) { return 'V11' }
    if ($minor -eq 25) { return 'V9' }
    if ($minor -ge 23) { return 'V8B' }
    if ($minor -eq 22) { return 'V8A' }
    if ($minor -eq 21) { return 'V7' }
    if ($minor -eq 20) { return 'V5' }
    if ($minor -ge 16) { return 'V4' }
    if ($minor -ge 3) { return 'V3' }
    return 'V2'
}

function Invoke-Tool([string]$exe, [string]$arguments) {
    Write-Host "> $exe $arguments"
    $process = Start-Process -FilePath $exe -ArgumentList $arguments -Wait -NoNewWindow -PassThru
    if ($process.ExitCode -ne 0) {
        Write-Host "The tool failed with exit code $($process.ExitCode)."
        exit 1
    }
}

# ---- what goes into the pak ----

$files = @(Get-ChildItem -LiteralPath $Stage -Recurse -File | Where-Object { $_.Name -ne $Placeholder })
if ($files.Count -eq 0) {
    Write-Host "Nothing to pack. Put the cooked files under:"
    Write-Host "  $(Join-Path $Stage "$Project\Content")"
    Write-Host "at the same path they have in the game."
    exit 1
}
$top = @(Get-ChildItem -LiteralPath $Stage -Directory | ForEach-Object { $_.Name })
if ($top -contains 'Content') {
    Write-Host "Warning: $PakName\Content exists. Content belongs one level down: $PakName\$Project\Content."
}
Write-Host "Packing $($files.Count) file(s) from $Stage"

# ---- the .pak ----

New-Item -ItemType Directory -Force -Path $Dist | Out-Null
Get-ChildItem -LiteralPath $Dist -File | Where-Object { $_.BaseName -eq $PakName } | Remove-Item -Force
$pakDir = $Dist
if ($IoStore) {
    # For IoStore the plain pak is only an intermediate: retoc converts it.
    $pakDir = Join-Path $Dist 'legacy'
    New-Item -ItemType Directory -Force -Path $pakDir | Out-Null
}
$pak = Join-Path $pakDir "$PakName.pak"
if (Test-Path -LiteralPath $pak) { Remove-Item -LiteralPath $pak -Force }

$unrealPak = Find-Tool 'UnrealPak' 'UNREALPAK'
$repak = Find-Tool 'repak' 'REPAK'
if ($unrealPak) {
    # UnrealPak response file: one line per file, "source on disk" "path inside the pak".
    # Paths inside the pak start with ../../../ which is the game's top folder.
    $list = Join-Path $Dist 'filelist.txt'
    $lines = foreach ($file in $files) {
        $relative = $file.FullName.Substring($Stage.Length).TrimStart('\').Replace('\', '/')
        '"' + $file.FullName + '" "../../../' + $relative + '"'
    }
    [IO.File]::WriteAllLines($list, [string[]]$lines, [Text.Encoding]::Unicode)
    Invoke-Tool $unrealPak ('"' + $pak + '" -create="' + $list + '"')
}
elseif ($repak) {
    # repak packs a whole folder as it is, with the same ../../../ mount point. Give it a clean copy
    # so the placeholder text file stays out of the pak.
    $copy = Join-Path (Join-Path $Dist 'stage') $PakName
    if (Test-Path -LiteralPath $copy) { Remove-Item -LiteralPath $copy -Recurse -Force }
    foreach ($file in $files) {
        $destination = Join-Path $copy $file.FullName.Substring($Stage.Length).TrimStart('\')
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
    }
    Invoke-Tool $repak ('pack --version ' + (Get-RepakVersion $EngineVersion) + ' "' + $copy + '" "' + $pak + '"')
}
else {
    Write-Host "No packer found. Put UnrealPak.exe or repak.exe into the tools folder here, or on PATH."
    Write-Host "UnrealPak.exe is in Engine\Binaries\Win64 of an Unreal Engine install (use the game's engine version)."
    Write-Host "repak is at https://github.com/trumank/repak/releases"
    exit 1
}
if (-not (Test-Path -LiteralPath $pak)) {
    Write-Host "The packer did not write $pak"
    exit 1
}

# ---- IoStore: .pak to .utoc + .ucas + .pak ----

if ($IoStore) {
    $retoc = Find-Tool 'retoc' 'RETOC'
    if (-not $retoc) {
        Write-Host "retoc.exe not found. Put it into the tools folder here, or on PATH."
        Write-Host "retoc is at https://github.com/trumank/retoc/releases"
        exit 1
    }
    $utoc = Join-Path $Dist "$PakName.utoc"
    $versionParts = $EngineVersion.Split('.')
    $retocVersion = 'UE' + $versionParts[0] + '_' + $versionParts[1]
    Invoke-Tool $retoc ('to-zen "' + $pak + '" "' + $utoc + '" --version ' + $retocVersion)
}

$built = @(Get-ChildItem -LiteralPath $Dist -File | Where-Object { $_.BaseName -eq $PakName })
Write-Host "Built:"
foreach ($file in $built) { Write-Host "  $($file.FullName)" }
if ($NoInstall) { exit 0 }

# ---- install ----

if (-not $Game) {
    $pathFile = Join-Path $Here 'game-path.txt'
    if (Test-Path -LiteralPath $pathFile) {
        $Game = [IO.File]::ReadAllText($pathFile, [Text.Encoding]::UTF8).Trim()
    }
}
if (-not $Game) {
    Write-Host "Not installed: no game folder. Put it on the first line of game-path.txt, or pass it to pack.bat."
    exit 0
}
$paks = Join-Path $Game "$Project\Content\Paks"
if (-not (Test-Path -LiteralPath $paks)) {
    $found = @(Get-ChildItem -LiteralPath $Game -Directory | ForEach-Object { Join-Path $_.FullName 'Content\Paks' } | Where-Object { Test-Path -LiteralPath $_ })
    if ($found.Count -ne 1) {
        Write-Host "Not installed: no $Project\Content\Paks folder under $Game"
        exit 1
    }
    $paks = $found[0]
    Write-Host "Warning: the game's project folder is not '$Project'. Installing into $paks, but the paths inside the pak must start with that folder's name."
}
$gameHasUtoc = @(Get-ChildItem -LiteralPath $paks -File -Filter '*.utoc').Count -gt 0
if ($gameHasUtoc -and -not $IoStore) {
    Write-Host "Warning: this game uses IoStore (.utoc files in Content\Paks). Assets in a plain .pak are ignored there; only loose files such as .ini work. Generate the project again with the IoStore format."
}
if (-not $gameHasUtoc -and $IoStore) {
    Write-Host "Warning: this game has no .utoc files in Content\Paks, so it does not use IoStore. Generate the project again with the plain pak format."
}
if (@(Get-ChildItem -LiteralPath $paks -File -Filter '*.sig').Count -gt 0) {
    Write-Host "Note: this game ships .sig files next to its paks. If the mod is ignored, the game checks pak signatures; see the README."
}

$target = Join-Path $paks $ModsFolder
New-Item -ItemType Directory -Force -Path $target | Out-Null
foreach ($file in $built) {
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $target $file.Name) -Force
}
Write-Host "Installed to: $target"
