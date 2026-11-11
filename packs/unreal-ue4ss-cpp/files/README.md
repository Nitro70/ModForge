# {{ModName}}

A UE4SS C++ mod{{#if GameName}} for {{GameName}}{{/if}}, built against
{{#if Ue4ss == stable}}
UE4SS 3.0.1 (the stable release) with CMake.
{{#elif Ue4ss == xmake}}
the `experimental-latest` UE4SS (the 2024 experimental builds) with xmake.
{{#else}}
UE4SS `experimental2` (the current experimental line) with CMake.
{{/if}}

A C++ mod is compiled together with UE4SS itself, so this folder is a small workspace: UE4SS's source as a git
submodule, and your mod next to it.

## Layout

| File | What it is for |
|---|---|
{{#if Ue4ss == xmake}}
| `xmake.lua` | The workspace: includes `RE-UE4SS` first, then the mod. |
| `{{ProjectName}}\xmake.lua` | The mod's build target. The `ue4ss.mod` rule comes from RE-UE4SS. |
{{#else}}
| `CMakeLists.txt` | The workspace: adds `RE-UE4SS` first, then the mod. |
| `{{ProjectName}}\CMakeLists.txt` | The mod's build target: a dll that links against the `UE4SS` target. |
{{/if}}
| `{{ProjectName}}\dllmain.cpp` | The mod. A class derived from `RC::CppUserModBase`, and the two exports UE4SS looks for: `start_mod` and `uninstall_mod`. |
| `RE-UE4SS\` | Not here yet: `setup.bat` adds it as a git submodule. |
| `setup.bat` | Run once. `git init`, adds the submodule, checks out the right UE4SS version, fetches its own submodules. |
| `build.bat` | Configures and builds in `Game__Shipping__Win64`, the configuration released UE4SS builds use. |
| `install.bat`, `install.ps1` | Copy the built dll into the game as `dlls\main.dll`. |
| `game-path.txt` | The game folder `install.bat` copies to. One line. |

## What you need

- Visual Studio with the "Desktop development with C++" workload
{{#if Ue4ss == stable}}
  (an MSVC that supports C++20 with `std::format`).
- CMake 3.18 or newer and the Rust toolchain 1.73 or newer.
{{#elif Ue4ss == xmake}}
  (version 17.9 or newer: C++23).
- xmake 2.9.3 or newer and the Rust toolchain 1.73 or newer.
{{#else}}
  (version 17.13 or newer: C++23).
- CMake 3.22 or newer and the Rust toolchain 1.73 or newer.
{{/if}}
- Git.
- A GitHub account that is linked to an Epic Games account (accept the invite to the EpicGames organisation)
  and has an SSH key. UE4SS's `deps/first/Unreal` submodule is a private repo of Unreal headers; without
  access `setup.bat` stops with "Permission denied" or "Repository not found".

## Build

```
setup.bat      once
build.bat      after every change
```

{{#if Ue4ss == xmake}}
`build.bat` runs `xmake f -m "Game__Shipping__Win64"` and `xmake`. The dll is
`Binaries\Game__Shipping__Win64\{{ProjectName}}\{{ProjectName}}.dll`. For a Visual Studio solution run
`xmake project -k vsxmake2022` and open `vsxmake2022\{{ProjectName}}Workspace.sln`.
{{#elif Ue4ss == stable}}
`build.bat` runs `cmake -S . -B Output -G "{{Generator}}"` and builds the `{{ProjectName}}` target in
`Game__Shipping__Win64`. The solution is `Output\{{ProjectName}}Workspace.sln`; `install.bat` finds
`{{ProjectName}}.dll` under `Output`.
{{#else}}
`build.bat` runs `cmake -B build -G "{{Generator}}"` and builds the `{{ProjectName}}` target in
`Game__Shipping__Win64`. The solution is `build\{{ProjectName}}Workspace.sln`; `install.bat` finds
`{{ProjectName}}.dll` under `build`.
{{/if}}

The first build compiles all of UE4SS and takes several minutes. Later builds only compile the mod.

## Where it goes in the game

```
{{#if GamePath && UnrealBinDir}}
{{GamePath}}\{{UnrealBinDir}}\
{{#else}}
<Game>\<Project>\Binaries\Win64\
{{/if}}
    UE4SS.dll                  (newer installs: ue4ss\UE4SS.dll)
    Mods\                      (newer installs: ue4ss\Mods\)
        mods.txt
        {{ProjectName}}\
            dlls\
                main.dll       <- {{ProjectName}}.dll, renamed
{{#if EnabledTxt}}
            enabled.txt
{{/if}}
```

`install.bat` does exactly this: it creates `Mods\{{ProjectName}}\dlls`, copies the newest built
`{{ProjectName}}.dll` there as `main.dll`{{#if EnabledTxt}} and writes an empty `enabled.txt`{{#else}} and adds `{{ProjectName}} : 1` to `mods.txt`{{/if}}.
{{#if !GamePath}}
Write the game's folder on the first line of `game-path.txt` first, or run `install.bat "D:\Games\MyGame"`.
{{/if}}
Close the game before installing: it keeps `main.dll` open while it runs.

## enabled.txt or mods.txt

- **`mods.txt`**: a line `{{ProjectName}} : 1` in `Mods\mods.txt`, above the `Keybinds : 1` entry at the bottom.
  Mods start in the order of the file.
- **`enabled.txt`**: an empty file of that name in the mod folder. No edit to `mods.txt`; these mods start
  after the `mods.txt` ones, in no fixed order.

## Check that it loaded

`UE4SS.log` (next to `UE4SS.dll`) should have, near the top:

```
[{{ProjectName}}] loaded {{Version}}
```

and, once UE4SS has finished scanning the game, a second line that starts with
`[{{ProjectName}}] Object Name:` and ends in `/Script/CoreUObject.Object`.

## When it does not load

- **"Could not find the dlls folder for mod {{ProjectName}}":** the dll is not in a `dlls` subfolder.
- **A "Failed to load" line that names {{ProjectName}}:** the file is not called `main.dll`, or it was built against
  another UE4SS than the one in the game. The mod and UE4SS must come from the same UE4SS version and the
  same configuration (`Game__Shipping__Win64`), because they share C++ classes and the C runtime. Check the
  version on the first lines of `UE4SS.log` and rebuild against that tag (`setup.bat` holds the tag).
- **The game crashes at start with the mod in place:** the same mismatch, or a `Debug` build of the mod in a
  Shipping UE4SS.
- **Nothing about the mod in the log:** it is not enabled (no `enabled.txt`, no `{{ProjectName}} : 1` line), or
  the folder is one level too deep.
- **The first mod in `mods.txt` never starts (UE4SS 3.0.1):** the stock file begins with a byte-order mark.
  Save it as UTF-8 without BOM.
- **`setup.bat` fails on `deps/first/Unreal`:** the GitHub account has no access to the Unreal headers. Link
  it to Epic Games, accept the organisation invite and add an SSH key.
- **A crash the first time the mod touches an object:** nothing from the `Unreal` namespace may be used before
  `on_unreal_init` has run. The constructor is too early.

## Traps

- Objects from the game's content (`/Game/...`) are freed when their map unloads. Do not keep pointers to them
  across maps; find them again. A new world can get the old world's address, so comparing addresses does not
  prove it is the same map.
- Searching all objects is slow and unsafe while a map is loading. Do it when something happens, not every tick.
- A UE4SS update means a rebuild: point `setup.bat` at the new tag, run it and `build.bat` again.
