# {{ModName}}

A BepInEx 6 plugin for a Unity IL2CPP game{{#if GameName}} ({{GameName}}){{/if}}.

An IL2CPP game is compiled to native code: the game folder has `GameAssembly.dll` and no
`Assembly-CSharp.dll`. BepInEx 6 generates .NET "interop" versions of the game's classes into
`BepInEx\interop` the first time the game starts, and this project compiles against those.

## Layout

| File | What it is for |
|---|---|
| `{{ProjectName}}.csproj` | The project: net6.0, the BepInEx package, references to the interop dlls{{#if Deploy && GamePath}}, and the copy into the game after a build{{/if}}. |
| `Plugin.cs` | The entry point. BepInEx creates the `Plugin` class and calls `Load` once when the game starts. |
{{#if Component}}
| `ModBehaviour.cs` | A MonoBehaviour with `Update` and a hotkey. `Plugin.Load` registers it with IL2CPP and attaches it. |
{{/if}}
{{#if Harmony}}
| `Patches/ExamplePatch.cs` | A Harmony patch skeleton: change what a game method does. |
{{/if}}
| `NuGet.Config` | Adds the BepInEx package feed. Without it the restore fails. |
{{#if !GamePath}}
| `lib/` | Put the dlls from the game's `BepInEx\interop` folder here to use Unity and game classes. |
{{/if}}

## Build

1. Install BepInEx 6 in the game: a bleeding edge build from https://builds.bepinex.dev/projects/bepinex_be,
   the `Unity.IL2CPP` zip that matches the game's exe (`win-x64` for a 64-bit game, `win-x86` for 32-bit).
   Unzip it into the folder with the game's exe.
2. Start the game once and wait. The first start takes a few minutes while BepInEx generates
   `BepInEx\interop`. Close the game.
{{#if !GamePath}}
3. Copy every dll from `<game folder>\BepInEx\interop` into `lib\`.
4. Build:
{{#else}}
3. Build:
{{/if}}

```
dotnet build -c Release
```

The plugin is `bin\Release\net6.0\{{ProjectName}}.dll`.

The project also builds before the interop dlls exist, with a warning: everything inside `#if HAS_INTEROP`
(the parts that need Unity or game classes) is left out until the interop dlls are found
in `{{#if GamePath}}{{GamePath}}\BepInEx\interop{{#else}}lib\{{/if}}`.

## Install

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
  plugin can load and nothing reports it. Check that `winhttp.dll`, `doorstop_config.ini` and the `dotnet`
  folder sit next to the game's exe, and that the BepInEx build matches the exe: a 64-bit game silently
  ignores a 32-bit `winhttp.dll` and the other way round. A game update can switch a game from 32-bit to
  64-bit, which unloads BepInEx without any message; reinstall the matching build.
- **BepInEx 5 is installed instead of 6:** BepInEx 5 only works on Mono games. An IL2CPP game needs a
  BepInEx 6 `Unity.IL2CPP` build.
- **The log exists but has no line from this plugin:** the dll is not under `BepInEx\plugins`, or the log has a
  "Could not load" error for it a few lines after the chainloader starts.
- **"Could not load file or assembly BepInEx.Unity.IL2CPP":** the game has a much older BepInEx 6
  (`6.0.0-pre.1`), where that dll was still called `BepInEx.IL2CPP`. Update BepInEx in the game, or set the
  package version in the csproj to the build the game has (the first lines of `LogOutput.log` show it).
- **It worked until the game updated:** BepInEx regenerates `BepInEx\interop` after an update. Build again
  against the new dlls{{#if !GamePath}} (copy them into `lib\` again){{/if}}. In games with obfuscated names the class and method names can
  change with every update.
{{#if Component}}
- **`Load` runs but the hotkey does nothing:** check the log for the warning that the plugin was built without
  the interop assemblies. If it was built with them and `Input.GetKeyDown` throws, the game only has Unity's
  new Input System; read the key through `UnityEngine.InputSystem.Keyboard.current` instead (that class is in
  `Unity.InputSystem.dll` in the interop folder).
- **A component you wrote fails to attach:** it needs the `(IntPtr ptr) : base(ptr)` constructor, and it must be
  registered before use. `AddComponent<T>()` in `Plugin.Load` does the registering for you;
  `gameObject.AddComponent<T>()` on some other object needs `ClassInjector.RegisterTypeInIl2Cpp<T>()` first.
{{/if}}
{{#if Harmony}}
- **A Harmony patch applies but never runs:** the IL2CPP compiler inlines small methods (getters, setters,
  short helpers) into their callers, so nothing calls the method you patched any more. There is no error.
  Patch a caller one level up, or change the data the method reads.
{{/if}}
- **Game collections are not .NET collections:** a game `List<T>` is `Il2CppSystem.Collections.Generic.List<T>`
  and a game array is `Il2CppReferenceArray<T>` or `Il2CppStructArray<T>`. Passing a .NET list where the game
  expects its own fails at run time, not at build time.
- **The plugin GUID** (`{{PluginGuid}}`) names the config file and is how other mods refer to this one.
  Do not change it after release.
