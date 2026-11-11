# {{ModName}}

A Factorio mod by {{Author}}, version {{Version}}, for Factorio {{FactorioVersion}}.

## Layout

The folder name is part of the mod: `{{ModId}}_{{Version}}` is the `name` and the `version` from `info.json`
joined by an underscore. If either differs from `info.json`, Factorio does not load the mod.

| File | What it is for |
|---|---|
| `info.json` | Name, version, title, author, game version and dependencies. Without it the folder is not a mod. |
| `settings.lua` | Stage 1: defines the mod's settings. Here: one on/off setting. |
{{#if Item}}
| `data.lua` | Stage 2: defines prototypes (items, recipes, entities). Here: one item and its recipe. |
{{#else}}
| `data.lua` | Stage 2: defines prototypes (items, recipes, entities). Empty for now. |
{{/if}}
| `control.lua` | Stage 3: runtime code that reacts to events while a save is played. |
| `locale/en/locale.cfg` | The English names and messages. Without an entry the game shows `Unknown key`. |
| `README.md` | This file. The game ignores it. |

The game looks for exactly these file names at the top of the mod folder. `data.lua` in a subfolder is
never run unless another file `require`s it.

## Install

Nothing to build. Copy the whole folder to the mods folder, which is in your user data, not in the game
folder:

```
%APPDATA%\Factorio\mods\{{ModId}}_{{Version}}
```

`info.json` must be directly inside that folder. Start Factorio: the mod is listed under Mods on the main
menu, and it is enabled.

## See it working

- Start a new game. The chat shows `{{ModName}} {{Version}} is loaded.`
{{#if Item}}
- Open the crafting menu: among the intermediate products there is a blue gear, `{{ModName}} gadget`,
  made from 2 iron plates.
{{/if}}
{{#if Events}}
- Once a minute the chat shows how many things players have built.
{{/if}}
- Settings, Mod settings, Map has `Show the welcome message`.
- `%APPDATA%\Factorio\factorio-current.log` has the line `{{ModId}} {{Version}}: on_init`.

## Releasing

To share the mod, zip the folder so that the zip is named `{{ModId}}_{{Version}}.zip` and contains the
folder. For a new version change `version` in `info.json` and rename the folder to match.

## When it does not load

- **Not in the mod list:** the folder is not directly in `%APPDATA%\Factorio\mods`, the folder name is not
  `{{ModId}}_{{Version}}` (or just `{{ModId}}`), or `info.json` is not valid JSON.
- **Listed but red or disabled:** `factorio_version` does not match the game's major version, or a
  dependency is missing.
- **The game stops at startup with an error naming the mod:** an error in `settings.lua` or `data.lua`.
  The message gives the file and line. A prototype that misses a required field is reported the same way.
- **The game starts but the save reports an error:** an error in `control.lua`.
- **`Unknown key` instead of a name:** the entry is missing in `locale/en/locale.cfg`, or it is under the
  wrong `[section]`. Items go under `[item-name]`, settings under `[mod-setting-name]`.
- **A value is gone after loading a save:** it was kept in a local variable. Keep game state in the
  `storage` table.
- **Names:** prototype and setting names are shared by all mods. Keep the `{{ModId}}-` prefix on
  everything you add, or two mods silently overwrite each other.
