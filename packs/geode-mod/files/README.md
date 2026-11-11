# {{ModName}}

A Geometry Dash mod for the Geode loader by {{Author}}, version {{Version}}.

Built for Geode {{GeodeVersion}} and Geometry Dash {{GdVersion}}.

## Layout

| File | What it is for |
|---|---|
| `mod.json` | The mod's identity: ID `{{ModId}}`, name, version, and the exact Geode and Geometry Dash versions it is built for. Read at build time and by the loader. |
| `CMakeLists.txt` | The build. Finds the Geode SDK through the `GEODE_SDK` environment variable and packs the result. |
| `src/main.cpp` | The code: a log line when the mod loads, and a hook on the main menu that adds a button. |
| `about.md` | The long description shown when a player opens the mod in game. |
| `changelog.md` | The list of changes per version, shown in game. |
| `.gitignore` | Keeps build output out of git. |
| `README.md` | This file. Not part of the built mod. |

`mod.json`, `about.md` and `changelog.md` must stay at the top of the project: the build looks for them
there by name. A `logo.png` next to them becomes the mod's icon in game. There is none here, so the mod
shows the default icon until you add one.

## Build

You need, once:

1. A C++ compiler (Visual Studio with the "Desktop development with C++" workload on Windows) and CMake.
2. The Geode CLI, then the SDK: `geode sdk install`, then `geode sdk install-binaries`.
   This sets the `GEODE_SDK` environment variable. Open a new terminal afterwards.
3. Optional: `geode config setup` to tell the CLI where Geometry Dash is, so builds are installed for you.

Then, in this folder:

```
geode build
```

or without the CLI:

```
cmake -B build
cmake --build build --config RelWithDebInfo
```

The result is `build\{{ModId}}.geode`.

## Install

Geode itself must be installed in the game first (the game folder then has `Geode.dll` and a `geode`
folder).

If you set up a CLI profile, `geode build` already copied the mod into the game. Otherwise copy
`{{ModId}}.geode` to:

```
{{#if GamePath}}
{{GamePath}}\geode\mods\{{ModId}}.geode
{{#else}}
<Geometry Dash folder>\geode\mods\{{ModId}}.geode
{{/if}}
```

## See it working

Start Geometry Dash. The row of buttons at the bottom of the main menu has one more, with a thumbs-up
icon. Click it for a `Hello from {{ModName}}!` box. The newest file in `geode\logs` in the game folder has
the line `{{ModName}} {{Version}} loaded`.

The example needs the `geode.node-ids` mod, which gives the game's menus their names. It is listed under
`dependencies` in `mod.json`, and Geode downloads it for the player.

## When it does not load

- **CMake stops with "Unable to find Geode SDK":** `GEODE_SDK` is not set in this terminal. Run
  `geode sdk install` and open a new terminal.
- **The build stops with a version error:** `geode` in `mod.json` must be the version of the SDK you build
  with. The SDK's version is in the `VERSION` file in the folder `GEODE_SDK` points to. Change `mod.json`
  to it, or update the SDK with `geode sdk update`.
- **The mod is in the list but marked as not loadable:** `gd` in `mod.json` does not match the game's
  version, or the mod was built for a newer Geode than the one installed in the game. Both must match
  exactly: update the versions in `mod.json`, update the SDK, and rebuild.
- **The mod is not in the list at all:** the `.geode` file is not in `geode\mods`, or the game runs without
  Geode.
- **A hook does nothing:** the function in the `$modify` class does not have exactly the game function's
  name, return type and parameters, or the `Geode/modify/<Class>.hpp` header for that class is not
  included.
- **The button is missing but the log line is there:** `geode.node-ids` is not installed or is switched
  off, so `getChildByID("bottom-menu")` finds nothing. The log then says so.
- **The mod ID** (`{{ModId}}`) names the mod's save data and settings and is how other mods refer to it.
  Lowercase letters, digits, dots, dashes and underscores only. Do not change it after release.
