# {{ModName}}

A code mod for Vintage Story.

## Layout

| File | What it is for |
|---|---|
| `{{ProjectName}}.csproj` | The project: target framework, references to the game's dlls, and the steps that gather the mod into one folder and zip it. |
| `modinfo.json` | What the game reads first: type, mod id, name, version. A mod without it at its top level is not loaded. The file name is fixed. |
| `{{SystemClass}}.cs` | The entry point. The game creates every `ModSystem` class and calls `Start` when a world loads. |
| `assets/{{ModId}}/lang/en.json` | The English text, read with `Lang.Get("{{ModId}}:key")`. |
| `Properties/launchSettings.json` | Lets Visual Studio and Rider start the game with this mod and the debugger attached. |

The folder under `assets` must be the mod id (`{{ModId}}`): it is the mod's domain, the part before the colon
in every asset name. Other folders the game knows go beside `lang`: `blocktypes`, `itemtypes`, `recipes`,
`shapes`, `textures`, `sounds`.

## Where the game is

The project finds the game's dlls through `VINTAGE_STORY`, the folder with `Vintagestory.exe`
(by default `%APPDATA%\Vintagestory`).

{{#if GamePath}}
The csproj sets it to:

```
{{GamePath}}
```

Change `<VINTAGE_STORY>` in `{{ProjectName}}.csproj` if the game moves.
{{#else}}
No game folder was set, so it has to come from an environment variable named `VINTAGE_STORY`. To create it,
run this once in PowerShell, then restart the editor or terminal:

```
[Environment]::SetEnvironmentVariable("VINTAGE_STORY", "$Env:AppData\Vintagestory", "User")
```

Or put the folder into the csproj instead, in the first `<PropertyGroup>`:

```xml
<VINTAGE_STORY>C:\Users\you\AppData\Roaming\Vintagestory</VINTAGE_STORY>
```
{{/if}}

## Build

You need the .NET SDK that matches the target framework (`{{Tfm}}`) and the game installed.

```
dotnet build -c Release
```

The build makes two things:

- `bin\Release\Mods\mod\` with `{{ProjectName}}.dll`, `modinfo.json` and `assets`: the mod as a folder,
- `bin\Release\{{ModId}}_{{Version}}.zip`: the same, zipped, with `modinfo.json` at the top level of the zip.

## Install

Copy `{{ModId}}_{{Version}}.zip` into the game's mod folder, without unpacking it:

```
%APPDATA%\VintagestoryData\Mods
```

That is the data folder, not the install folder (`%APPDATA%\Vintagestory`), although the install folder has a
`Mods` folder too: that one holds the game's own parts.

While developing, skip the copy: start the **Client** profile from the editor. It runs the game with
`--addModPath` pointing at `bin\<configuration>\Mods`. Do not also keep the zip in the Mods folder then, or
the game finds the same mod id twice.

## Check that it loaded

The mod is listed in the game's Mod Manager. After a world loads,
`%APPDATA%\VintagestoryData\Logs\client-main.log` and `server-main.log` have a line ending in:

```
[{{ModId}}] {{ModName}} loaded: hello world!
```

## When it does not load

- **Not in the Mod Manager at all:** the zip was unpacked into a folder inside a folder, or `modinfo.json` is
  not at the top level of the zip, or the file is in the install folder's `Mods` and not in
  `VintagestoryData\Mods`.
- **Listed with an error about `modinfo.json`:** a missing comma or quote, or a `modid` with anything other
  than lowercase letters and digits.
- **"Could not load file or assembly" or the mod's dll is skipped:** the target framework does not match the
  game. Game 1.22 and newer run on .NET 10 (`net10.0`), game 1.21 on .NET 8 (`net8.0`). Change
  `TargetFramework` in the csproj and rebuild.
- **`Lang.Get` returns the key (`{{ModId}}:hello`):** the folder under `assets` is not named after the mod id,
  or `assets` did not make it into the zip.
- **Build error "VintagestoryAPI.dll was not found":** see "Where the game is" above.
- **Joining a server fails with a mod mismatch:** server and client need the same mod version.
- **The mod id** (`{{ModId}}`) is how the game, saves and other mods know this mod. Do not change it after
  release.
