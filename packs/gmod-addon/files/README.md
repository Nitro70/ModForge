# {{ModName}}

A Garry's Mod addon by {{Author}}, version {{Version}}.

## Layout

Garry's Mod decides what a Lua file is, and where it runs, from the folder it is in. Keep every file and
folder name lowercase: Linux servers and the Workshop packer are case sensitive.

| File | What it is for |
|---|---|
| `lua/autorun/sh_{{Id}}.lua` | Shared. Runs on the server and on every client when Lua starts. |
| `lua/autorun/server/sv_{{Id}}.lua` | Server only. Never sent to players. |
| `lua/autorun/client/cl_{{Id}}.lua` | Client only. Runs on each player's PC. |
{{#if Swep}}
| `lua/weapons/weapon_{{Id}}.lua` | A scripted weapon. The file name is its class name, `weapon_{{Id}}`. |
{{/if}}
{{#if Entity}}
| `lua/entities/{{Id}}_crate/shared.lua` | A scripted entity, the part both sides load. The folder name is its class name, `{{Id}}_crate`. |
| `lua/entities/{{Id}}_crate/init.lua` | The entity on the server: model, physics, what Use does. |
| `lua/entities/{{Id}}_crate/cl_init.lua` | The entity on the client: drawing. |
{{/if}}
{{#if Tool}}
| `lua/weapons/gmod_tool/stools/{{Id}}.lua` | A tool for the tool gun. The file name is the tool's mode name, `{{Id}}`. |
{{/if}}
| `addon.json` | Title, type and tags for the Workshop. Only read when the addon is packed with gmad or uploaded. The game does not need it to load the folder. |
| `README.md` | This file. Listed under `ignore` in `addon.json`, so it is left out of a Workshop upload. |

The `sh_`, `sv_` and `cl_` prefixes are only a habit that makes the side visible in the name. What decides
the side is the folder. File names in `lua/autorun` are shared by all addons: two addons with the same
file name replace each other, which is why every name here contains `{{Id}}`.

## Install

Nothing to build. Copy the whole `{{Id}}` folder into the game's addons folder:

```
{{#if GamePath}}
{{GamePath}}\garrysmod\addons\{{Id}}
{{#else}}
<Steam library>\steamapps\common\GarrysMod\garrysmod\addons\{{Id}}
{{/if}}
```

The `lua` folder must be directly inside `{{Id}}`. One folder too many (`addons\{{Id}}\{{Id}}\lua`) and
nothing loads.

## See it working

1. Start Garry's Mod and a sandbox game.
2. Open the console (enable it under Options, Keyboard, Advanced, then press the `~` key). It shows:
   `[{{ModName}}] shared file loaded on the server`, `[{{ModName}}] server file loaded`,
   `[{{ModName}}] shared file loaded on the client` and `[{{ModName}}] client file loaded`.
3. The chat shows `[{{ModName}}] version {{Version}} loaded.` once you are in the map.
4. Type `{{Id}}_hello` in the console: the server answers in chat.
{{#if Swep}}

The weapon: hold Q for the spawn menu, Weapons tab, category `{{ModName}}`. Left click pushes the prop
you look at, right click prints a line in chat.
{{/if}}
{{#if Entity}}

The entity: hold Q, Entities tab, category `{{ModName}}`, spawn `{{ModName}} Crate` and press E on it.
{{/if}}
{{#if Tool}}

The tool: hold Q, pick `{{ModName}}` in the tool list under Construction. Left click paints a prop red,
right click puts the colour back.
{{/if}}

In a single player game, saving a Lua file reloads it while the game runs. A file that is new needs a
map change before the game sees it.

## When it does not load

- **No lines in the console:** the folder is nested one level too deep, or it is not in
  `garrysmod\addons`.
- **Works in single player, breaks on a server:** a file name has an uppercase letter, or a client file
  was never sent. The server must call `AddCSLuaFile` for every file a client runs. The shared file here
  does that for itself and for the client file.
- **"attempt to index global" on one side only:** the code is running on the side that does not have that
  function. `util.AddNetworkString` exists only on the server, `chat.AddText` only on the client. Wrap code
  in `if SERVER then` or `if CLIENT then`, or move it to the right folder.
- **A net message does nothing:** its name was not registered with `util.AddNetworkString` on the server
  before `net.Start`.
{{#if Swep}}
- **The weapon is not in the spawn menu:** `SWEP.Spawnable` is not `true`, or the file is not directly in
  `lua/weapons`.
{{/if}}
{{#if Entity}}
- **The entity is an error model or invisible:** `init.lua` did not `AddCSLuaFile` the client files, so
  the client never got `cl_init.lua`.
{{/if}}
{{#if Tool}}
- **The tool is missing from the list:** the file must be in `lua/weapons/gmod_tool/stools`, lowercase.
  Change the map after adding it.
- **The tool name shows as `#tool.{{Id}}.name`:** the `language.Add` lines must use the file name
  exactly.
{{/if}}
- **Uploading to the Workshop fails:** `type` in `addon.json` must be one of `ServerContent`, `gamemode`,
  `map`, `weapon`, `vehicle`, `npc`, `tool`, `effects`, `model`, `entity`, and `tags` holds at most two of
  `fun`, `roleplay`, `scenic`, `movie`, `realism`, `cartoon`, `water`, `comic`, `build`.
