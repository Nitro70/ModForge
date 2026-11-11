# {{ModName}}

A Godot Mod Loader mod{{#if GameName}} for {{GameName}}{{/if}} (Godot {{Godot}}). Mod ID: `{{ModId}}`.

It only works in a game that has the Godot Mod Loader: either the developer built it in (Brotato, Dome
Keeper...) or it was added to the game with the loader's self-setup.

## Layout

| File | What it is for |
|---|---|
| `mods-unpacked\{{ModId}}\` | The mod. The folder name is the mod's ID: `namespace-name` from the manifest, joined by a hyphen. |
| `mods-unpacked\{{ModId}}\manifest.json` | Name, namespace, version and compatibility. The loader refuses a mod whose manifest is missing a required field. |
| `mods-unpacked\{{ModId}}\mod_main.gd` | The entry point. The loader creates it as a node and calls `_init()`, later `_ready()`. The file must have this name. |
{{#if Extension}}
| `mods-unpacked\{{ModId}}\extensions\example_extension.gd` | A script extension: changes the game script `{{VanillaScript}}`. Installed from `mod_main.gd`. |
{{/if}}
| `build-zip.bat`, `build-zip.ps1` | Check the manifest, build the zip into `dist\`, copy it into the game. |
| `game-path.txt` | The folder with the game's exe. One line. |

## The zip

Players install a mod as one zip. The Mod Loader mounts the zip over the game's files, so the zip must contain
the `mods-unpacked` folder itself:

```
{{ModId}}-{{Version}}.zip
    mods-unpacked/
        {{ModId}}/
            manifest.json
            mod_main.gd
{{#if Extension}}
            extensions/
                example_extension.gd
{{/if}}
```

`build-zip.bat` builds exactly that, named `{{ModId}}-<version>.zip` with the version taken from
`manifest.json`. Zipping the `{{ModId}}` folder by hand, without `mods-unpacked` around it, gives a zip the loader
ignores.

## Install

The zip goes into a `mods` folder next to the game's exe:

```
{{#if GamePath}}
{{GamePath}}\mods\{{ModId}}-{{Version}}.zip
{{#else}}
<Game>\mods\{{ModId}}-{{Version}}.zip
{{/if}}
```

{{#if GamePath}}
`build-zip.bat` copies it there (and removes older versions of this mod from that folder).
{{#else}}
Write the game's folder on the first line of `game-path.txt` and `build-zip.bat` copies it there.
{{/if}}
`build-zip.bat -NoInstall` only builds. Games with Steam Workshop mods also load zips from the Workshop
folder, `steamapps\workshop\content\<app id>\<item id>\`.

## Check that it loaded

The Mod Loader writes `logs\modloader.log` in the game's user data folder, by default
`%APPDATA%\Godot\app_userdata\<Game name>\logs\modloader.log`. Look for:

```
{{ModId}}:Main: Ready! Version {{Version}}
```

## Working in the Godot editor

To run the mod with the game's project open in Godot {{Godot}} (a decompiled copy of the game, or the developer's
modding project), copy `mods-unpacked\{{ModId}}` into the project's `res://mods-unpacked\` folder. In the editor
the loader reads mods from there directly, with no zip.

A mod with images, sounds or scenes needs their imported versions in the zip too
({{#if Godot == 4}}`.godot/imported/`{{#else}}`.import/`{{/if}}). The Mod Tool editor plugin exports such a mod correctly; `build-zip.bat` packs
only what is in the mod folder, which is enough for scripts, JSON and translations.

## When it does not load

- **Nothing about the mod in `modloader.log`, or no such log:** the game has no Mod Loader, the zip is not in
  the `mods` folder next to the exe, or the zip does not have `mods-unpacked/{{ModId}}/` at its top level.
- **"Manifest is missing required fields":** `manifest.json` must have `name`, `namespace`, `version_number`,
  `website_url`, `description`, `dependencies` and `extra`, and under `extra.godot`: `authors`,
  `compatible_mod_loader_version` and `compatible_game_version`. Empty values are fine, missing keys are not.
- **"Invalid name or namespace":** both take letters, digits and `_` only, and at least 3 characters. No
  spaces, no hyphens. The hyphen only joins the two in the folder name.
- **The folder name and the manifest disagree:** the folder must be exactly `{{ModId}}`. After renaming the mod,
  change the folder, the manifest and the constants at the top of `mod_main.gd` together. `build-zip.bat`
  checks this.
- **"Invalid semantic version":** `version_number` and the versions in the two `compatible_...` lists are
  three numbers without leading zeros (`1.0.0`, not `1.0` or `01.0.0`).
{{#if Godot == 4}}
- **"incompatible with the current game version":** `compatible_game_version` lists `{{GameVersion}}`. When the
  game reports a higher major version than every entry, the mod is refused. Set it to the game's version.
{{/if}}
- **JSON errors:** a trailing comma or a comment in `manifest.json` makes the whole file unreadable.
{{#if Extension}}
- **The extension does nothing:** the path after `extends` in `example_extension.gd` must be the exact
  `res://` path of the game's script; the loader logs an error when it is not. Scripts the game loads with
  `preload()` cannot be extended{{#if Godot == 4}}, and neither can scripts with a `class_name` (use script hooks for those){{/if}}.
{{/if}}
- **Two mods use the same constant names:** the constants in `mod_main.gd` carry the mod's ID
  (`{{ConstPrefix}}_...`) for that reason. Keep them unique.
