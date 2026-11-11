# {{ModName}}

A SMAPI mod for Stardew Valley.

## Layout

| File | What it is for |
|---|---|
| `{{ProjectName}}.csproj` | The project. It targets .NET 6 (what Stardew Valley 1.6 runs on) and pulls in the official build package `Pathoschild.Stardew.ModBuildConfig`, which references the game and SMAPI and installs the mod after a build. |
| `manifest.json` | What SMAPI reads first. Without it, or with a wrong `EntryDll`, the mod is skipped. It must sit next to the dll. |
| `ModEntry.cs` | The entry point. SMAPI creates the one class that inherits `Mod` and calls `Entry` once. |
{{#if Config}}
| `ModConfig.cs` | The settings. SMAPI writes them to `config.json` in the installed mod's folder on the first run. |
{{/if}}
{{#if Config && Gmcm}}
| `IGenericModConfigMenuApi.cs` | The part of Generic Mod Config Menu's API used to show the settings in game. The other mod stays optional. |
{{/if}}
{{#if I18n}}
| `i18n/default.json` | The English text. A translation is another file beside it, such as `i18n/de.json`, with the same keys. |
{{/if}}
{{#if Harmony}}
| `Patches/ExamplePatch.cs` | A Harmony patch: code that runs after a game method. |
{{/if}}

A folder named `assets` (images, json, maps) is also copied into the installed mod when you add one.

## Build

You need the .NET SDK (6 or newer) and Stardew Valley with [SMAPI](https://smapi.io) installed. The build
package cannot work without the game: there is no other source for the game's dlls.

```
dotnet build
```

The build does three things:

1. compiles `{{ProjectName}}.dll`,
2. copies the mod to `Mods\{{ProjectName}}` in the game folder (`{{ProjectName}}.dll`, `manifest.json`{{#if I18n}}, `i18n`{{/if}}),
3. writes `{{ProjectName}} {{Version}}.zip` into `bin\Debug\net6.0`, ready to upload to a mod site.

### Where the game is

{{#if GamePath}}
The csproj points at:

```
{{GamePath}}
```

Change `<GamePath>` in `{{ProjectName}}.csproj` if the game moves. When that folder does not exist the build
package falls back to looking for a standard Steam or GOG install.
{{#else}}
No game folder was set, so the build package looks for a standard Steam or GOG install by itself. If it
stops with "The mod build package can't find your game folder", add this line to the first `<PropertyGroup>`
in `{{ProjectName}}.csproj`, with the folder that holds `Stardew Valley.exe`:

```xml
<GamePath>C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley</GamePath>
```
{{/if}}

To set it once for every mod project on this PC instead, create `stardewvalley.targets` in your user folder
(`%USERPROFILE%`) with a `<Project><PropertyGroup><GamePath>...</GamePath></PropertyGroup></Project>` inside.

## Run

Start the game through SMAPI (`StardewModdingAPI.exe`, or the Steam launch option the SMAPI installer gave
you). The SMAPI console lists the mod under "Loaded mods" and then shows a line like:

```
[12:00:00 INFO  {{ModName}}] {{ModName}} {{Version}} loaded
```

Load a save and press **F9** to see a message in the corner of the screen.{{#if Config}} The key and the other
settings are in `Mods\{{ProjectName}}\config.json`, which appears after the first run.{{/if}}

## Version

The version is `<Version>` in the csproj and nowhere else. `manifest.json` contains `%ProjectVersion%`, which
the build replaces with that number in the installed copy and in the zip. That is why the `manifest.json` in
this folder must not be copied into the game by hand: SMAPI rejects `%ProjectVersion%` as a version.

## When it does not load

- **The mod is not in SMAPI's list at all:** `Mods\{{ProjectName}}` has no `manifest.json` directly inside it,
  or the folder sits one level too deep (`Mods\{{ProjectName}}\{{ProjectName}}\manifest.json`).
- **"its DLL '{{ProjectName}}.dll' doesn't exist":** `EntryDll` in `manifest.json` does not match the dll name.
  The dll is named after `<AssemblyName>` in the csproj; keep the two the same.
- **"manifest.json is invalid":** a missing comma or quote, or `%ProjectVersion%` left in a manifest that was
  copied by hand.
- **"needs SMAPI x or later":** `MinimumApiVersion` is higher than the installed SMAPI.
- **The game started without mods:** it was started from `Stardew Valley.exe` and not through SMAPI. No SMAPI
  console window means no mods.
- **Build error "can't find your game folder" or "doesn't contain SMAPI":** see "Where the game is" above, and
  install SMAPI into that game folder.
- **Build error about files in use:** the game is running. Close it, then build.
- **The `UniqueID`** (`{{UniqueId}}`) is how SMAPI, other mods and update checks know this mod. Do not change
  it after release.
{{#if Config && Gmcm}}
- **No page in Generic Mod Config Menu:** that mod is not installed, or a method in
  `IGenericModConfigMenuApi.cs` no longer matches the original. SMAPI prints the mismatch in its console.
{{/if}}
{{#if Harmony}}
- **The patch does nothing:** the game renamed or changed the patched method. Harmony reports that in the
  SMAPI console when the mod starts.
{{/if}}
