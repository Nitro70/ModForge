# {{ModName}}

A BepInEx 5 plugin{{#if GameName}} for {{GameName}}{{/if}}.

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

BepInEx 5 must be installed in the game first (the game folder then has `BepInEx\` and `winhttp.dll`, and
`BepInEx\plugins` exists after the game has been started once).

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

- **No line in the log at all:** the dll is not under `BepInEx\plugins`, or BepInEx itself is not running
  (no `LogOutput.log` means BepInEx did not start).
- **"Could not load file or assembly":** wrong target framework for the game's Unity version. Change
  `TargetFramework` in the csproj (`netstandard2.1` for Unity 2021.2 and newer, `net46` for 2018 to 2021.1,
  `net35` for older).
- **The game is IL2CPP** (it has `GameAssembly.dll` and no `{{#if GameDataDir}}{{GameDataDir}}{{#else}}<Game>_Data{{/if}}\Managed\Assembly-CSharp.dll`):
  BepInEx 5 cannot load there. Use a BepInEx 6 IL2CPP or MelonLoader project instead.
- **Awake runs but Update never does:** some games destroy the BepInEx manager object. Set
  `HideManagerGameObject = true` under `[Chainloader]` in `BepInEx\config\BepInEx.cfg`.
- **The plugin GUID** (`{{PluginGuid}}`) names the config file and is how other mods refer to this one.
  Do not change it after release.
