# {{ModName}}

A MelonLoader mod for a Unity {{#if Backend == il2cpp}}IL2CPP{{#else}}Mono{{/if}} game{{#if GameName}} ({{GameName}}){{/if}}.

{{#if Backend == il2cpp}}
An IL2CPP game is compiled to native code: the game folder has `GameAssembly.dll` and no
`Assembly-CSharp.dll`. MelonLoader generates .NET versions of Unity and the game's classes into
`MelonLoader\Il2CppAssemblies` the first time the game starts, and this project compiles against those.
{{#else}}
A Mono game keeps its code in `<Game>_Data\Managed\Assembly-CSharp.dll`, and this project compiles against
the dlls in that folder.
{{/if}}

## Layout

| File | What it is for |
|---|---|
| `{{ProjectName}}.csproj` | The project: target framework, references to MelonLoader and the game's dlls{{#if Deploy && GamePath}}, and the copy into the game after a build{{/if}}. |
| `Core.cs` | The entry point. It holds the `MelonInfo` and `MelonGame` attributes MelonLoader looks for, and the `Core` class whose `OnInitializeMelon` runs once when the game starts. |
{{#if Harmony}}
| `Patches/ExamplePatch.cs` | A Harmony patch skeleton: change what a game method does. |
{{/if}}
| `NuGet.Config` | Adds the package feed used while the project compiles against NuGet packages instead of an installed MelonLoader. |
{{#if !GamePath}}
{{#if Backend == il2cpp}}
| `lib/` | Put the dlls from the game's `MelonLoader\Il2CppAssemblies` folder here to use Unity and game classes. |
{{#else}}
| `lib/` | Put the game's `Assembly-CSharp.dll` here to use game classes. |
{{/if}}
{{/if}}

## Build

1. Install MelonLoader in the game with the installer from https://github.com/LavaGang/MelonLoader/releases.
   The game folder then has `version.dll` and a `MelonLoader` folder.
2. Start the game once and close it. MelonLoader creates the `Mods` folder{{#if Backend == il2cpp}} and generates
   `MelonLoader\Il2CppAssemblies`, which takes a few minutes the first time{{/if}}.
{{#if !GamePath && Backend == il2cpp}}
3. Copy every dll from `<game folder>\MelonLoader\Il2CppAssemblies` into `lib\`.
4. Build:
{{#else}}
3. Build:
{{/if}}

```
dotnet build -c Release
```

The mod is `bin\Release\{{Tfm}}\{{ProjectName}}.dll`.

{{#if GamePath}}
The project also builds before MelonLoader is installed: it then compiles against the `LavaGang.MelonLoader`
NuGet package instead of the dlls in `{{GamePath}}\MelonLoader`.
{{#else}}
No game folder was set, so MelonLoader itself comes from the `LavaGang.MelonLoader` NuGet package.
{{/if}}
{{#if Backend == il2cpp}}
Until the generated assemblies are found in `{{#if GamePath}}{{GamePath}}\MelonLoader\Il2CppAssemblies{{#else}}lib\{{/if}}` the build prints a warning and leaves
out everything inside `#if HAS_UNITY` (the parts that need Unity or game classes).
{{/if}}

## Install

{{#if Deploy && GamePath}}
Every build copies the dll to:

```
{{GamePath}}\Mods\{{ProjectName}}.dll
```

{{#else}}
Copy `{{ProjectName}}.dll` to `<game folder>\Mods\`.

{{/if}}
Start the game. The MelonLoader console and `MelonLoader\Latest.log` list the mod by name, followed by the
line `{{ModName}} {{Version}} loaded`.

## When it does not load

- **No MelonLoader console and `MelonLoader\Latest.log` is older than your last launch:** MelonLoader itself is
  not running, so no mod can load and nothing reports it. Check that `version.dll` sits next to the game's exe
  and matches it: a 64-bit game silently ignores a 32-bit `version.dll` and the other way round. A game
  update can switch a game from 32-bit to 64-bit; run the MelonLoader installer again.
- **The dll is in the wrong folder:** mods go in `<game folder>\Mods`. `Plugins` is for a different kind of dll
  (`MelonPlugin`), and a mod placed there is not started. `UserLibs` is for libraries that mods depend on.
- **The log names the mod but says it is for another game:** the `MelonGame` attribute in `Core.cs` does not
  match. Copy both strings from `{{#if GameDataDir}}{{GameDataDir}}{{#else}}<Game>_Data{{/if}}\app.info` exactly (line 1 is the developer, line 2 the game), or use
  `[assembly: MelonGame]` with no arguments to load everywhere.
- **The log does not name the mod at all:** the `MelonInfo` attribute is missing or points at the wrong class.
  It must name the class that inherits `MelonMod`.
{{#if Backend == il2cpp}}
- **The game is Mono, not IL2CPP** (it has `<Game>_Data\Managed\Assembly-CSharp.dll` and no `GameAssembly.dll`):
  this project is net6.0 and built against generated assemblies, which a Mono game does not have. Make a
  Mono project instead.
- **It worked until the game updated:** MelonLoader regenerates `MelonLoader\Il2CppAssemblies` after an update.
  Build again against the new dlls{{#if !GamePath}} (copy them into `lib\` again){{/if}}.
- **A game class is not found when compiling:** in the generated assemblies the game's namespaces get an
  `Il2Cpp` prefix (`Il2CppScheduleOne.Money`, or plain `Il2Cpp` for classes without a namespace). Unity's
  own classes keep their names.
{{#else}}
- **The game is IL2CPP, not Mono** (it has `GameAssembly.dll` and no `<Game>_Data\Managed\Assembly-CSharp.dll`):
  a Mono mod cannot load there. Make an IL2CPP project instead.
- **"Could not load file or assembly" or a missing method at start:** wrong target framework for the game's
  Unity version. Change `TargetFramework` in the csproj (`netstandard2.1` for Unity 2021.2 and newer,
  `net472` for 2018.1 to 2021.1, `net35` for older).
{{/if}}
{{#if Hotkey}}
- **The mod loads but the hotkey does nothing:** {{#if Backend == il2cpp}}check the log for the warning that the mod was built without
  the generated assemblies. If it was built with them and{{#else}}if{{/if}} `Input.GetKeyDown` throws an error every frame, the
  game only has Unity's new Input System; read the key through `UnityEngine.InputSystem.Keyboard.current`
  instead (that class is in `Unity.InputSystem.dll`).
{{/if}}
{{#if Harmony}}
- **A Harmony patch does nothing:** MelonLoader applies `[HarmonyPatch]` classes by itself, so a missing
  `PatchAll` is never the cause. Look in the log for an error naming the patch class: a wrong class or
  method name is reported there.{{#if Backend == il2cpp}} If there is no error, the method was probably inlined: the IL2CPP compiler
  folds small methods (getters, setters, short helpers) into their callers, so the patch applies and then
  never runs. Patch a caller one level up. Transpilers do not work on IL2CPP games at all.{{/if}}
{{/if}}
{{#if Config}}
- **Preferences do not show up in `UserData\MelonPreferences.cfg`:** they are written when the game closes
  normally. Killing the game from the task manager skips the save.
{{/if}}
