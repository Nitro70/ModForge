# {{ModName}}

A BepInEx 6 plugin for a Unity Mono game{{#if GameName}} ({{GameName}}){{/if}}.

BepInEx 6 is the bleeding edge line of BepInEx. Its plugins are not interchangeable with BepInEx 5 ones:
this project only loads in a game that has BepInEx 6 installed (`BepInEx\core\BepInEx.Unity.Mono.dll` exists).

## Layout

| File | What it is for |
|---|---|
| `{{ProjectName}}.csproj` | The project: target framework, BepInEx packages, references to the game's dlls{{#if Deploy && GamePath}}, and the copy into the game after a build{{/if}}. |
| `Plugin.cs` | The entry point. BepInEx creates the `Plugin` class and calls `Awake` once when the game starts. |
{{#if Harmony}}
| `Patches/ExamplePatch.cs` | A Harmony patch skeleton: change what a game method does. |
{{/if}}
| `NuGet.Config` | Adds the BepInEx package feed. Without it the restore fails. |
{{#if !GamePath}}
| `lib/` | Put the game's `Assembly-CSharp.dll` here to use game classes. |
{{/if}}

## Build

```
dotnet build -c Release
```

The plugin is `bin\Release\{{Tfm}}\{{ProjectName}}.dll`.

## Install

BepInEx 6 must be installed in the game first: a bleeding edge build from
https://builds.bepinex.dev/projects/bepinex_be, the `Unity.Mono` zip that matches the game's exe (`win-x64` for
a 64-bit game, `win-x86` for 32-bit), unzipped into the folder with the game's exe. The game folder then has
`BepInEx\` and `winhttp.dll`, and `BepInEx\plugins` exists after the game has been started once.

{{#if Deploy && GamePath}}
Every build copies the dll to:

```
{{GamePath}}\BepInEx\plugins\{{ProjectName}}\{{ProjectName}}.dll
```

{{#else}}
Copy `{{ProjectName}}.dll` to `<game folder>\BepInEx\plugins\{{ProjectName}}\`.

{{/if}}
Start the game, then open `BepInEx\LogOutput.log` and look for `{{ModName}} {{Version}} loaded`.

## When it does not load

- **No `BepInEx\LogOutput.log`, or it is older than your last launch:** BepInEx itself is not running, so no
  plugin can load and nothing reports it. Check that `winhttp.dll` and `doorstop_config.ini` sit next to the
  game's exe and that the BepInEx build matches the exe: a 64-bit game silently ignores a 32-bit
  `winhttp.dll` and the other way round. A game update can switch a game from 32-bit to 64-bit; reinstall
  the matching build.
- **The log exists but has no line from this plugin:** the dll is not under `BepInEx\plugins`, or the game has
  BepInEx 5. BepInEx 5 has `BepInEx\core\BepInEx.dll` and skips this plugin, because it was built against
  `BepInEx.Core.dll` and `BepInEx.Unity.Mono.dll`. Either install BepInEx 6 or make a BepInEx 5 project.
- **"Could not load file or assembly":** wrong target framework for the game's Unity version. Change
  `TargetFramework` in the csproj (`netstandard2.1` for Unity 2021.2 and newer, `net46` for 2018 to 2021.1,
  `net35` for older).
- **The game is IL2CPP** (it has `GameAssembly.dll` and no `{{#if GameDataDir}}{{GameDataDir}}{{#else}}<Game>_Data{{/if}}\Managed\Assembly-CSharp.dll`):
  a Mono plugin cannot load there. Make a BepInEx 6 IL2CPP or MelonLoader project instead.
- **Awake runs but Update never does:** some games destroy the BepInEx manager object. Set
  `HideManagerGameObject = true` under `[Chainloader]` in `BepInEx\config\BepInEx.cfg`.
{{#if Hotkey || Gui}}
- **A key only works while no other key is held:** that is how BepInEx's `KeyboardShortcut.IsDown` and
  `IsPressed` behave, by design. This project reads a plain `KeyCode` through `UnityInput.Current` instead;
  keep it that way for keys used while moving.
{{/if}}
{{#if Publicize}}
- **CS0507 "cannot change access modifiers when overriding":** the game assembly is publicized, so every
  member looks public. An override of a protected game method must be written `public override`.
{{/if}}
- **The plugin GUID** (`{{PluginGuid}}`) names the config file and is how other mods refer to this one.
  Do not change it after release.
