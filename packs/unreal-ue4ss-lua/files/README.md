# {{ModName}}

A UE4SS Lua mod{{#if GameName}} for {{GameName}}{{/if}}. This folder is the mod: UE4SS loads it from its `Mods` folder.

## Layout

| File | What it is for |
|---|---|
| `Scripts\main.lua` | The mod. UE4SS runs it once when the mod starts. The folder must be called `Scripts` and the file `main.lua`. |
{{#if EnabledTxt}}
| `enabled.txt` | An empty file. Its presence tells UE4SS to load the mod, with no line in `mods.txt`. |
{{/if}}
| `install.bat`, `install.ps1` | Copy the mod into the game. Not part of the mod. |
| `game-path.txt` | The game folder `install.bat` copies to. One line. |

## Where it goes in the game

UE4SS lives next to the game's real exe, which is not the small exe in the top folder:

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
            Scripts\
                main.lua
{{#if EnabledTxt}}
            enabled.txt
{{/if}}
```

`install.bat` finds that `Mods` folder and copies `Scripts`{{#if EnabledTxt}} and `enabled.txt`{{/if}} into `Mods\{{ProjectName}}`.
To do it by hand, copy this whole folder into `Mods`.

## Install

1. UE4SS must already be in the game (a `Mods` folder with `mods.txt` in it exists once it is).
{{#if GamePath}}
2. Run `install.bat`.
{{#else}}
2. Write the game's folder on the first line of `game-path.txt`, then run `install.bat`.
   Or run `install.bat "D:\Games\MyGame"`.
{{/if}}
3. Start the game.

## enabled.txt or mods.txt

UE4SS starts a mod in one of two ways:

- **`mods.txt`**: a line `{{ProjectName}} : 1` in `Mods\mods.txt`. Mods start in the order of the file, so this is the
  way to control load order. Put the line above the `Keybinds : 1` entry at the bottom. `: 0` turns a mod off.
- **`enabled.txt`**: an empty file of that name inside the mod folder. No edit to `mods.txt`. These mods start after
  the `mods.txt` ones, in no fixed order.

{{#if EnabledTxt}}
This mod uses `enabled.txt`. To switch to `mods.txt`, delete `enabled.txt` here and run `install.bat` again: it
then adds the line to `mods.txt` for you.
{{#else}}
This mod uses `mods.txt`: `install.bat` adds `{{ProjectName}} : 1` above the Keybinds entry. To switch, create an
empty `enabled.txt` in this folder and run `install.bat` again.
{{/if}}

## Check that it loaded

Open `UE4SS.log` (next to `UE4SS.dll`) after the game has reached its menu. You should see:

```
Starting Lua mod '{{ProjectName}}'
[Lua] [{{ProjectName}}] loaded {{Version}}
```

{{#if Keybind}}
Press {{Key}} in the game: the log gets `[{{ProjectName}}] {{Key}} pressed`.
{{/if}}
{{#if ConsoleCommand}}
Open the game's console and type `{{CommandName}}`: the reply appears in the console and in the log.
{{/if}}

To see the log live, set `GuiConsoleEnabled = 1` and `GuiConsoleVisible = 1` under `[Debug]` in
`UE4SS-settings.ini`. The window that opens also has a "Restart All Mods" button, which reloads `main.lua`
without restarting the game.

## When it does not load

- **No "Starting Lua mod '{{ProjectName}}'" line:** the folder is in the wrong place, or nothing enables it. The path
  must be exactly `Mods\{{ProjectName}}\Scripts\main.lua`. A second folder level
  (`Mods\{{ProjectName}}\{{ProjectName}}\Scripts`) is the usual mistake after unzipping.
- **The log says "disabled in mods.txt":** the line reads `{{ProjectName}} : 0`. Change it to `1`.
- **There is no `UE4SS.log` at all:** UE4SS itself is not running. Its files (`dwmapi.dll` plus `UE4SS.dll`, or
  `dwmapi.dll` plus a `ue4ss` folder) must be in `Binaries\Win64` next to the `-Shipping.exe`, not in the game's
  top folder.
- **The first mod in `mods.txt` never starts (UE4SS 3.0.1):** the stock `mods.txt` begins with a byte-order mark,
  which becomes part of the first mod's name. Save the file as UTF-8 without BOM. `install.bat` warns about it.
- **An error in the log right after "Starting Lua mod":** a Lua error in `main.lua`. The line number is in
  the message. Nothing after the failing line has run.
- **The console command does nothing:** the game's console must exist. `ConsoleEnablerMod : 1` in `mods.txt`
  creates it.

## Traps in UE4SS Lua

- **Threads:** key bind callbacks run on UE4SS's own thread. Calling a game function there crashes sooner or
  later. Wrap the work in `ExecuteInGameThread`. Hook and console command callbacks already run in the game.
- **Stale objects:** an object from the game's content (`/Game/...`) is freed when its map unloads. `IsValid()`
  on a freed object reads freed memory and can crash. Do not keep such objects in variables: look them up again.
  Engine classes (`/Script/...`) stay for the whole run.
- **A new map can reuse the old map's address,** so "same address" does not prove "same world". Compare the
  player controller too, or keep nothing between maps.
- **Searching:** `FindFirstOf`, `FindAllOf` and `FindObjects` walk every object. Never call them every frame or
  inside a hook that fires often; right after a map load that has crashed games.
- **Load-map hooks (3.0.1):** `RegisterLoadMapPreHook/PostHook`, `RegisterInitGameStatePreHook/PostHook` and
  `RegisterBeginPlayPreHook/PostHook` are unsafe in UE4SS 3.0.1. Hook
  `/Script/Engine.PlayerController:ClientRestart` instead to notice a new map.
- **Blueprint hooks run after the function,** and only once the Blueprint is loaded. To change what a
  Blueprint function reads, hook a function it calls earlier.
- **Registration limit (3.0.1):** keep `RegisterConsoleCommandHandler` plus `NotifyOnNewObject` calls well
  under 45 per mod. More than that has corrupted memory.
- **Arrays:** UE4SS 3.0.1 cannot safely append to a game array from Lua: writing to the index one past the
  end does not grow the array, it writes outside it. Change existing elements in place, or call a game
  function that adds the element.
- **Structs:** a Lua table works for a flat struct parameter (`{X=1, Y=2, Z=3}`). A struct inside a struct is
  not written from a table in 3.0.1.
- **Reloading:** "Restart All Mods" is fine for most edits. After changing a hook, restart the game to be
  sure the old hook is gone.
