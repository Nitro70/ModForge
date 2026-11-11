# {{ModName}}

A Trailmakers Lua mod by {{Author}}, version {{Version}}.

## Layout

| File | What it is for |
|---|---|
| `main.lua` | The mod. Trailmakers runs the file named exactly `main.lua` in the mod's folder, and nothing else on its own. |
| `README.md` | This file. The game ignores it. |

The game adds two folders next to `main.lua` the first time the mod runs: `data_static` (files the mod
can read with `tm.os.ReadAllText_Static`, uploaded with the mod) and
`data_dynamic_willNotBeUploadedToWorkshop` (files the mod can read and write with
`tm.os.ReadAllText_Dynamic` and `tm.os.WriteAllText_Dynamic`).

## Install

Nothing to build. Copy the whole `{{ModId}}` folder into the `mods` folder inside the game folder, so the
script ends up at:

```
{{#if GamePath}}
{{GamePath}}\mods\{{ModId}}\main.lua
{{#else}}
<Steam library>\steamapps\common\Trailmakers\mods\{{ModId}}\main.lua
{{/if}}
```

The `mods` folder already exists there and holds the game's own examples (`kickmod`, `spawnmod` and
others) and `trailmakers_docs.lua`.

## See it working

1. Start Trailmakers and host a game (mods run on the host).
2. Turn `{{ModId}}` on in the session's mod options.
3. Open `mods\{{ModId}}.log` in the game folder: it has the line `{{ModName}}: {{Version}} loaded` and one
   line for every player who joined.
{{#if Menu}}
4. Open the mod menu in game: it shows `{{ModName}} {{Version}}`, a counter and a `Press me` button.
{{/if}}
{{#if Hotkey}}
{{#if Menu}}
5. Press `{{Key}}`: a small message appears and the counter goes up.
{{#else}}
4. Press `{{Key}}`: a small message appears.
{{/if}}
{{/if}}

After changing `main.lua`, turn the mod off and on again in the session's mod options to reload it.

## Rules that keep it working

- **The API reference is `mods\trailmakers_docs.lua`** in the game folder. It is written by the game
  itself, so it matches your game version. Online pages can be out of date.
- **A key callback is found by name**, so the function must be global and the name passed to
  `tm.input.RegisterFunctionToKeyDownCallback` must be spelled the same.
- **Key names are lowercase words**: `home`, `end`, `insert`, `page up`, `page down`, `left shift`, `space`,
  single letters and digits. There are no function keys and no escape. Letters also fire while the player
  types in chat, and the builder already uses many of them (`x` deletes, `c` copies).
- **Player slots are reused.** Clear what you stored for a player in `OnPlayerLeft`, and do not bind a
  key twice for the same slot.
- **The game recycles block objects.** A value set on one block (engine power, for example) stays on that
  object when it is repaired, rebuilt or handed to another player. A mod that changes blocks has to put
  the values back itself.
- **There is no Lua on a normal PC to test with.** Check the syntax of `main.lua` with any Lua 5.2 tool
  before starting the game: a syntax error means the mod does nothing at all.

## When it does not load

- **Not in the mod list:** the folder is not directly inside the game's `mods` folder, or the script is
  not named `main.lua`.
- **In the list, but nothing happens and the log is empty or stops early:** an error in `main.lua`. The
  error is written to `mods\{{ModId}}.log`.
- **The menu or hotkey works for you but not for a friend:** only the host runs mods. Everything a joined
  player sees has to be created for their `playerId`, which `OnPlayerJoined` does here.
{{#if Hotkey}}
- **The key does nothing:** the key name is not one the game knows, or the callback function is `local`.
  Some keys never reach mods in some game versions: try another one from the list above.
{{/if}}
