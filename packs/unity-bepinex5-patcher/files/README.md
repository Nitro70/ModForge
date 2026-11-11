# {{ModName}}

A BepInEx 5 preloader patcher{{#if GameName}} for {{GameName}}{{/if}}.

A patcher is not a plugin. It runs before the game's code is loaded, gets `{{TargetDll}}` as a
Mono.Cecil `AssemblyDefinition` and can rewrite it: add fields to a game class, add types, change method
bodies. Use it for changes that must exist before the game starts. To run code inside the game or to
hook a method with Harmony, make a BepInEx 5 plugin instead; the two are often shipped together as two dlls.

## Layout

| File | What it is for |
|---|---|
| `{{ProjectName}}.csproj` | The project: net35, the BepInEx and Mono.Cecil packages{{#if Deploy && GamePath}}, and the copy into the game after a build{{/if}}. |
| `Patcher.cs` | The patcher. BepInEx finds the `Patcher` class by its `TargetDLLs` property and its `Patch` method. |
| `NuGet.Config` | Adds the BepInEx package feed. Without it the restore fails. |

## Build

```
dotnet build -c Release
```

The patcher is `bin\Release\net35\{{ProjectName}}.dll`. Nothing else in that folder needs to be shipped.

## Install

BepInEx 5 must be installed in the game first (the game folder then has `BepInEx\` and `winhttp.dll`).

{{#if Deploy && GamePath}}
Every build copies the dll to:

```
{{GamePath}}\BepInEx\patchers\{{ProjectName}}.dll
```

{{#else}}
Copy `{{ProjectName}}.dll` to `<game folder>\BepInEx\patchers\`.

{{/if}}
Start the game, then open `BepInEx\LogOutput.log`. Near the top, before the plugins load, there are three lines:
`{{ModName}} {{Version}} loaded`, then BepInEx's own `Patching [...] with [{{Namespace}}.Patcher]`, then this
patcher's `Patching ... (N types)`.

To see what the patcher produced, set `DumpAssemblies = true` under `[Preloader]` in
`BepInEx\config\BepInEx.cfg`. The patched assembly is then written to `BepInEx\DumpedAssemblies` and can be
opened in dnSpy or ILSpy. Turn it off again afterwards.

## When it does not load

- **No lines from the patcher in the log:** the dll is not in `BepInEx\patchers`. A patcher placed in
  `BepInEx\plugins` is ignored, and so is a plugin placed in `BepInEx\patchers`. Neither is reported.
- **"loaded" is logged but "Patching" never is:** the name in `TargetDLLs` matches no file. It must be the
  exact file name of a dll in `{{#if GameDataDir}}{{GameDataDir}}{{#else}}<Game>_Data{{/if}}\Managed`, with `.dll`.
- **Nothing is logged after changing the class:** BepInEx recognises a patcher only by its shape. `TargetDLLs`
  must be a `public static` property of type `IEnumerable<string>` (declaring it as `string[]` or
  `List<string>` hides the patcher), and `Patch` must be `public static void` with one `AssemblyDefinition`
  parameter (or `ref AssemblyDefinition`).
- **The patch is skipped and the log warns that the assembly is already loaded:** something referenced the
  target before the preloader could patch it. Never reference the game's dlls from this project, and do not
  use game or Unity types in `Initialize` or `Patch`. Work with names and Cecil types only. `Finish` runs
  after everything is loaded and may touch them.
- **The game crashes or the target class breaks after patching:** the patched code is invalid. Dump the
  assembly (see above) and open it in a decompiler. Fields added to a class that Unity serializes change
  how saved scenes and prefabs load; mark such fields `NotSerialized`.
- **A plugin cannot see the members this patcher adds:** the plugin is compiled against the game's original
  dll. Reach the new members with reflection, or compile the plugin against the dumped assembly.
- **"Could not load file or assembly Mono.Cecil":** the patcher was built against a newer Mono.Cecil than the
  one in `BepInEx\core`. Set the package version in the csproj to match that file, and do not copy your own
  `Mono.Cecil.dll` into the game.
- **The game is IL2CPP** (it has `GameAssembly.dll` and no `{{#if GameDataDir}}{{GameDataDir}}{{#else}}<Game>_Data{{/if}}\Managed\Assembly-CSharp.dll`):
  there are no managed assemblies to patch and BepInEx 5 does not run there at all.
- **No `LogOutput.log`, or it is older than your last launch:** BepInEx itself is not running. Check that
  `winhttp.dll` and `doorstop_config.ini` sit next to the game's exe and that the BepInEx build matches the
  exe (x64 or x86). A game update can switch a game from 32-bit to 64-bit, which unloads BepInEx silently.
