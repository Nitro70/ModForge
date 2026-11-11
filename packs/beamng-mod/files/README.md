# {{ModName}}

A BeamNG.drive mod by {{Author}}, version {{Version}}.
{{#if Kind == ge}}
Kind: game engine Lua extension.
{{#elif Kind == vehicle}}
Kind: vehicle Lua extension.
{{#elif Kind == ui}}
Kind: UI app.
{{#else}}
Kind: vehicle part (jbeam).
{{/if}}

## Layout

Everything under `mod\` is the mod. Its folder tree mirrors the game's own tree, and the game finds files
by path only: a file one folder off is never read and nothing is reported.

| File | What it is for |
|---|---|
{{#if Kind == ge}}
| `mod\lua\ge\extensions\{{ExtName}}.lua` | The extension. The file name is the extension name: `{{ExtName}}`. |
| `mod\scripts\{{ExtName}}\modScript.lua` | Runs when the mod is activated and tells the game to load the extension. Without it the extension file is never loaded. |
{{#elif Kind == vehicle}}
| `mod\lua\vehicle\extensions\auto\{{ExtName}}.lua` | The extension. Every file in `extensions\auto\` is loaded into every vehicle, so no mod script is needed. |
{{#elif Kind == ui}}
| `mod\ui\modules\apps\{{AppName}}\app.json` | Tells the game the app exists: its name, size, and which directive draws it. |
| `mod\ui\modules\apps\{{AppName}}\app.js` | The AngularJS directive `{{AppDirective}}`: the app's code. |
| `mod\ui\modules\apps\{{AppName}}\app.html` | The app's markup, loaded by `templateUrl` in `app.js`. |
{{#else}}
| `mod\vehicles\common\{{ModId}}\{{ModId}}.jbeam` | One part, `{{ModId}}_ballast`, for the `roofbars_load` slot. `vehicles\common\` is read for every vehicle. |
{{/if}}
| `pack.ps1` | Builds `dist\{{ModId}}.zip` from `mod\`. Not part of the mod. |
| `README.md` | This file. Not part of the mod. |

## Build and install

Close the game first, then:

```
powershell -ExecutionPolicy Bypass -File pack.ps1 -Install
```

This builds `dist\{{ModId}}.zip` and copies it to the mods folder of the BeamNG user folder:

```
%LOCALAPPDATA%\BeamNG\BeamNG.drive\current\mods\{{ModId}}.zip
```

The user folder is not the game folder. Open it from the launcher with Manage User Folder if it is somewhere
else on your PC. Never put mods in the game's install folder.

Leave `-Install` off to only build the zip. While you are still changing things you can also skip the zip:
copy the contents of `mod\` to `mods\unpacked\{{ModId}}\` in the user folder and the game loads it from there.

## See it working

{{#if Kind == ge}}
1. Start the game. Open `beamng.log` in the user folder and search for `{{ExtName}}`: there is a line
   `{{ModName}} {{Version}} loaded`.
2. Load any map, open the console with the `~` key, make sure the dropdown at the bottom left says
   `GE - Lua`, and run `{{ExtName}}.hello()`. A message appears at the top left of the screen.
3. Edit `{{ExtName}}.lua`. `Ctrl+L` reloads game engine Lua.
{{#elif Kind == vehicle}}
1. Start the game and spawn a vehicle. A message `{{ModName}} is active in this vehicle` appears at the top
   left, and `beamng.log` in the user folder has a line `{{ModName}} {{Version}} loaded in vehicle ...` for
   every vehicle on the map.
2. Open the console with the `~` key, pick your vehicle in the dropdown at the bottom left (not `GE - Lua`),
   and run `print({{ExtName}}.speedKmh())`.
3. Edit `{{ExtName}}.lua`. `Ctrl+R` reloads the current vehicle and its Lua.
{{#elif Kind == ui}}
1. Start the game and load any map.
2. Press Escape, open UI Apps, choose to add an app and pick `{{ModName}}` from the list.
3. Drive: the app shows your speed.
4. Edit the files, then press `F5` to reload the UI.

The app list shows a thumbnail from `app.png` next to `app.json`. There is none in this project, so the entry
has no picture until you add a PNG with that name.
{{#else}}
1. Start the game and spawn a vehicle that has roof bars fitted (for example the Gavril D-Series with the
   Roof Bars part).
2. Open the parts menu (`Ctrl+W`). The load slot under the roof bars now offers `{{ModName}} Ballast`.
   Fit it.
3. The Tuning tab has a `Ballast Weight` slider under `{{ModName}}`.

The part has nodes, beams and collision triangles but no mesh, so it is invisible: turn on the node and beam
debug view (`Ctrl+B` cycles it) to see the box sitting on the bars. To give it a look, add a `"flexbodies"`
section and ship the `.dae` next to the jbeam.

The file is written as strict JSON so any JSON tool can check it. The game also accepts comments and missing
commas in jbeam, but strict JSON never hurts.
{{/if}}

## When it does not load

Read `beamng.log` in the user folder first and search for `{{ModId}}`. It has been the answer every time.

- **No `mountEntry` line for the zip:** the zip is not in the user folder's `mods` folder, or the mod is
  switched off in the in-game Mod Manager.
- **Mounted, but nothing happens:** the paths inside the zip are wrong. Open the zip: `lua`, `scripts`, `ui`
  or `vehicles` must be at the very top, not inside another folder, and entry names must use forward
  slashes. A zip made with Explorer's Send to, or with Compress-Archive on Windows PowerShell 5.1, can break
  either rule. `pack.ps1` checks both.
- **You rebuilt the zip while the game was running:** the game still lists the files but can no longer read
  them, and the log says `modified > zip` followed by `unable to read file`. The zip is fine. Restart the
  game, and only replace the zip while the game is closed (`pack.ps1 -Install` refuses otherwise).
{{#if Kind == ge}}
- **The extension never loads:** the name in `setExtensionUnloadMode('{{ExtName}}', 'manual')` must be the
  file name of the extension without `.lua`, and `modScript.lua` must sit in its own folder under
  `scripts\`. Rename one and you must rename the other.
- **`attempt to index global 'controller'` (or `electrics`, `powertrain`):** those only exist inside a
  vehicle. Game engine Lua reaches them with `getPlayerVehicle(0):queueLuaCommand('...')`.
{{#elif Kind == vehicle}}
- **The file is not picked up:** it must be directly in `lua\vehicle\extensions\auto\`. A file one level up,
  in `lua\vehicle\extensions\`, is only loaded when something asks for it by name.
- **Game engine functions are nil here:** a vehicle has its own Lua state. Reach the game engine side with
  `obj:queueGameEngineLua('...')`.
- **The console says the function does not exist:** the console dropdown is on `GE - Lua`. Switch it to the
  vehicle.
{{#elif Kind == ui}}
- **Listed but empty:** `directive` in `app.json`, the directive name in `app.js` and the dash form in
  `domElement` do not agree, or the folder name differs from the path in `templateUrl`.
- **Not listed at all:** `app.json` is not valid JSON, or the app folder is not directly under
  `ui\modules\apps\`.
- **Text with double curly braces shows raw:** an error in `app.js` stopped the directive. `Ctrl+U` opens
  the UI console, which shows the error.
{{#else}}
- **The part is not offered:** the slot is named by `slotType`. `roofbars_load` only exists on a vehicle that
  has roof bars fitted. To target another slot, find the slot name in that vehicle's own jbeam.
- **The part appears twice or replaces another one:** part names (the top-level key, here
  `{{ModId}}_ballast`) and node names are shared by all mods. Keep the `{{ModId}}` prefix on everything.
- **`unable to decode JSON` in the log:** a syntax error in the jbeam. Parse it with any JSON tool.
{{/if}}
- **BeamMP:** on a multiplayer server a mod does nothing unless the server serves it from
  `Resources/Client`. Event names sent between server and client must not contain a colon.
