# {{ModName}}

A Teardown mod by {{Author}}.
{{#if Kind == global}}
Kind: global script. It runs in every level while the mod is enabled.
{{#elif Kind == tool}}
Kind: tool. It adds one tool to every player's inventory.
{{#else}}
Kind: spawnable. It adds one object to the spawn menu.
{{/if}}

## Layout

The folder itself is the mod. Teardown finds everything by name, so keep the names.

| File | What it is for |
|---|---|
| `info.txt` | The mod's name, author, description and tags for the mod list. `version = 2` says the scripts use script version 2. Without `info.txt` the folder is not a mod. |
{{#if Kind == global}}
| `main.lua` | The script. A mod with a `main.lua` and no `main.xml` is a global mod: the script runs in every level. |
{{#elif Kind == tool}}
| `main.lua` | Registers the tool, reacts to the fire button on the server and plays the effects on each client. |
| `prefab\{{ModId}}.xml` | The tool as the player holds it: where it sits on screen, the model, the hand grip and the muzzle. |
{{#else}}
| `spawn.txt` | One line per spawnable: `file : CATEGORY/Name`. This is what puts it in the spawn menu. |
| `spawn\{{ModId}}.xml` | The prefab that is created when the player picks it. |
| `main.lua` | Empty on purpose. A multiplayer session can only enable a mod that has a `main.lua`. |
{{/if}}
{{#if Options && Kind != spawn}}
| `options.lua` | The options screen behind the Options button in the mod list. Saves one setting that `main.lua` reads. |
{{/if}}
| `README.md` | This file. The game ignores it. |

## Install

Nothing to build. Copy the whole `{{ModName}}` folder to:

```
%USERPROFILE%\Documents\Teardown\mods\{{ModName}}
```

Create `mods` if it does not exist yet. Do not put it in the game's own `mods` folder: that one belongs to
the game and Steam may overwrite it.

Start Teardown, open Mods on the main menu, find `{{ModName}}` among your local mods and enable it.
A new mod, and any change to a prefab or model, needs a restart of the game.

## See it working

{{#if Kind == global}}
Start any level. `{{ModName}} loaded. Press Alt+J.` is shown at the top of the screen for a few seconds.
Hold Alt and press J: the server counts the press and every player sees who pressed it.
{{#elif Kind == tool}}
Start any level and scroll through your tools to `{{ModName}}` (it is in the last tool group, 6).
It looks like the game's pistol. The left mouse button punches a hole where you aim.
{{#else}}
Start a sandbox level, open the spawn menu and look in the industrial props category for
`{{ModName}} Crate`. It spawns a wooden crate.

The category is the part before the slash in `spawn.txt`. `SPAWN_INDUSTRIAL` is one of the game's own
category keys, so the crate is listed beside the game's industrial props. Other keys the game ships with
include `SPAWN_FURNITURE`, `SPAWN_EXPLOSIVE` and `SPAWN_CAR`.
{{/if}}

## Rules that keep it working

- **Every script starts with `#version 2`.** It selects the multiplayer script model: `server.*` functions
  run on the host and do the physics (`Spawn`, `MakeHole`, `Delete`, `Explosion`), `client.*` functions run
  on every player's PC and do input, sound, particles and UI.
- **Both `server` and `client` exist everywhere.** `if server then` is always true and proves nothing. Put
  code in the function of the side it belongs to instead of testing which side you are on.
- **Check every game function against `data\script_defs.lua`** in the game folder. It is the API reference
  and has an example for each function. A misspelled or invented function is a nil call at run time.
  `Players()`, `PlayersAdded()` and `PlayersRemoved()` come from `script/include/player.lua`, which is why
  that file is included.
- **Sending values:** the server writes to the `shared` table and clients read it, or it calls
  `ClientCall(0, "client.name", ...)` to reach every player. A client reaches the server with
  `ServerCall("server.name", ...)`.
- **`GetTime()` is wall time** everywhere except `server.update`, where it is simulated time. Use `dt` or
  `update` when timing physics.
- **A body's transform is not where its voxels are.** For distances, use
  `TransformToParentPoint(GetBodyTransform(b), GetBodyCenterOfMass(b))`.

## When it does not load

- **Not in the mod list:** the folder is not directly under `Documents\Teardown\mods`, or `info.txt` is
  missing or misnamed.
- **Listed but nothing happens:** the mod is not enabled, or the script has an error. The game shows
  script errors on screen while the level runs.
{{#if Kind == global}}
- **The message never shows:** the first line of `main.lua` is no longer `#version 2`. Without it the
  `server.*` and `client.*` functions are never called.
{{#elif Kind == tool}}
- **The tool is drawn in the middle of your face:** the outer group in `prefab\{{ModId}}.xml` must be named
  `instance=MOD/prefab/{{ModId}}.xml`, the prefab's own path. Renaming the file means changing that name
  and the path in `RegisterTool` too.
- **The tool is missing from the inventory:** `SetToolEnabled` was never called for the player. A
  registered tool starts disabled for everyone.
- **The prefab stopped loading after an edit:** two hyphens in a row inside an XML comment make the whole
  file unparseable, and the game says nothing. Check it with
  `powershell -c "[xml](Get-Content 'prefab\{{ModId}}.xml' -Raw) | Out-Null"`.
- **A new model looks unchanged:** Teardown caches `.vox` files. Restart the game.
{{#else}}
- **Not in the spawn menu:** every line of `spawn.txt` is `path : CATEGORY/Name`, with the path relative to
  the mod folder and forward slashes.
- **It spawns but will not move:** extra attributes on the shape can make the body static. Keep
  `<body dynamic="true">` around it.
- **The prefab stopped loading after an edit:** two hyphens in a row inside an XML comment make the whole
  file unparseable, and the game says nothing. Check it with
  `powershell -c "[xml](Get-Content 'spawn\{{ModId}}.xml' -Raw) | Out-Null"`.
- **Cannot be enabled in a multiplayer session:** `main.lua` is missing. Keep the empty one.
- **A new model looks unchanged:** Teardown caches `.vox` files. Restart the game.
{{/if}}
