# {{ModName}}

A Balatro mod for Steamodded by {{Author}}, version {{Version}}.

## Layout

| File | What it is for |
|---|---|
| `{{ModId}}.json` | The mod's metadata. This file is how Steamodded finds the mod: it gives the id, the prefix and the Lua file to run. The file name itself does not matter, the `.json` ending and the contents do. |
{{#if Joker}}
| `main.lua` | The mod's code, named by `main_file` in the metadata. Registers one Joker. |
{{#else}}
| `main.lua` | The mod's code, named by `main_file` in the metadata. |
{{/if}}
| `README.md` | This file. The game ignores it. |

## Install

Balatro needs two things before any mod loads:

1. **Lovely**, the injector: `winmm.dll` in the Balatro game folder. When it works, a second window named
   Lovely opens next to the game.
2. **Steamodded**, the mod loader: the folder `%APPDATA%\Balatro\Mods\smods`.

Then copy the whole `{{ModId}}` folder to the same Mods folder, which is in your user data and not in the
game folder:

```
%APPDATA%\Balatro\Mods\{{ModId}}
```

`{{ModId}}.json` must be directly inside that folder.

## See it working

1. Start Balatro. The Lovely window shows `{{ModName}} {{Version}} loaded`.
2. Open Mods on the main menu: `{{ModName}}` is in the list.
{{#if Joker}}
3. Open Collection, Jokers, and go to the last page: `{{ModName}} Joker` is there, with a `{{ModName}}`
   badge under its description. It gives +4 Mult.
{{/if}}

After changing `main.lua`, hold `M` in game (or press `Alt+F5`) to restart the game quickly.
{{#if Joker}}

## Your own artwork

The example Joker borrows the game's plain Joker picture. To give it its own:

1. Draw the card at 71 x 95 pixels and save it as `assets\1x\{{Prefix}}_jokers.png`. Save the same
   picture at double size, 142 x 190, as `assets\2x\{{Prefix}}_jokers.png`. Both folders must have a file
   with the same name. A sheet with several cards is a grid of 71 x 95 cells.
2. Register the sheet in `main.lua`, before the Joker:

   ```lua
   SMODS.Atlas {
       key = "jokers",
       path = "{{Prefix}}_jokers.png",
       px = 71,
       py = 95
   }
   ```

3. Add `atlas = "jokers",` to the Joker. `pos` then counts cells in your sheet, starting with x 0, y 0 at
   the top left.
{{/if}}

## When it does not load

- **No Lovely window:** Lovely is not installed. `winmm.dll` belongs in the game folder, next to
  `Balatro.exe`. Antivirus programs often remove it.
- **Lovely window but no Mods button:** Steamodded is missing or nested one folder too deep. It must be
  `Mods\smods\<files>`, not `Mods\smods\smods\<files>`.
- **The mod is not in the list:** `{{ModId}}.json` is not directly inside `Mods\{{ModId}}`, or it is not
  valid JSON. All of `id`, `name`, `author`, `description`, `prefix` and `main_file` are required, and
  `author` must be a list.
- **Listed but greyed out with a message:** a dependency is not met. `Steamodded (>={{SteamoddedVersion}})`
  means that version or newer.
- **The game crashes on start with a Lua error:** the crash screen names the file and line. A mistake in
  `main.lua` stops the whole game from starting, not just the mod.
- **Listed twice or loaded twice:** a `--- STEAMODDED HEADER` line was added to a Lua file. Use the
  metadata file only.
- **Saves lose your cards after a rename:** `prefix` and each `key` are part of every card's identity
  (`j_{{Prefix}}_example`). Do not change them once the mod is in use.
