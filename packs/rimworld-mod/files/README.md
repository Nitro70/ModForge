# {{ModName}}

A RimWorld {{GameVersion}} mod{{#if !Code}} made of XML only{{/if}}.

## Layout

This folder is the mod. RimWorld finds a mod by its `About\About.xml` and then loads folders by their exact
names, so the names and places below are fixed.

| File | What it is for |
|---|---|
| `About/About.xml` | Name, author, `packageId` and supported versions. A folder under `Mods` without it is not a mod. The folder and file names are fixed. |
| `Defs/ThingDefs/{{ProjectName}}_Items.xml` | An example Def: a new item. The game reads every xml file under `Defs`, in any subfolder, under any name. |
{{#if Code}}
| `{{GameVersion}}/Assemblies/` | Where the game loads the mod's dll from when it runs version {{GameVersion}}. The build creates it. |
| `Source/{{ProjectName}}.csproj` | The C# project. It compiles against the `Krafs.Rimworld.Ref` package, so it builds without the game, and writes `{{ProjectName}}.dll` into `{{GameVersion}}/Assemblies`. |
| `Source/Startup.cs` | The entry point: a `[StaticConstructorOnStartup]` class the game runs once after loading. It writes the "loaded" line to the log{{#if Harmony}} and applies the Harmony patches{{/if}}. |
{{/if}}
{{#if Code && Settings}}
| `Source/{{ModClass}}.cs` | The `Mod` class: owns the settings and draws the settings window. |
| `Source/{{SettingsClass}}.cs` | The settings themselves and how they are saved. |
| `Languages/English/Keyed/{{ProjectName}}.xml` | The English text the code shows, looked up with `"Key".Translate()`. |
{{/if}}
{{#if Code && Harmony}}
| `Source/Patches/ExamplePatch.cs` | A Harmony patch: code that runs after a game method. |
{{/if}}

Other folders the game knows, to add when needed: `Textures` (png files, referred to by path without the
extension), `Sounds`, `Patches` (XML patches that change other Defs), and `About/Preview.png` (the picture in
the mod list, 640x360).

Folders directly in the mod (`Defs`, `Languages`, `Textures`) load on every game version. A folder named
after a version (`{{GameVersion}}`) loads only on that version and holds what differs between versions, which is
normally the dll. That is all the game needs; a `LoadFolders.xml` is only for layouts that go beyond this.
{{#if Code}}

## Build

You need the .NET SDK. The game does not have to be installed.

```
dotnet build Source -c Release
```

The result is `{{GameVersion}}\Assemblies\{{ProjectName}}.dll`, and nothing else is put there. That matters: any other
dll in `Assemblies` is loaded by the game too.{{#if Harmony}} In particular `0Harmony.dll` must not be shipped. It comes
from the Harmony mod, and a second copy breaks other mods.{{/if}}

The game's dlls are not referenced from the game folder. To compile against a newer game build, the
`Krafs.Rimworld.Ref` package version in the csproj is the thing to change.
{{/if}}

## Install

The mod has to be a folder inside the game's `Mods` folder:

```
{{#if GamePath}}
{{GamePath}}\Mods\{{ProjectName}}\About\About.xml
{{#else}}
<game folder>\Mods\{{ProjectName}}\About\About.xml
{{/if}}
```

{{#if Code && Deploy && GamePath}}
Every build copies this folder there (everything except `Source`). Change `<GameDir>` in the csproj if the
game moves. When this folder already is that folder, nothing is copied.
{{#else}}
Copy this whole folder there, or create it there in the first place and work in it. The game folder is the
one with `RimWorldWin64.exe`.{{#if Code}} `Source` can stay or go, the game ignores it.{{/if}}
{{/if}}

Then start RimWorld, open **Mods**, and enable {{ModName}}.{{#if Code && Harmony}} Enable **Harmony** too and keep it above this
mod; the game offers to get it when it is missing.{{/if}} The game restarts to apply the list.

## Check that it loaded

{{#if Code}}
Turn on **Development mode** in Options, then open the debug log (the `` ` `` key, or the first icon in the row
at the top of the screen). It has the line:

```
[{{ModName}}] {{Version}} loaded
```

{{#else}}
Turn on **Development mode** in Options. Errors from the mod's XML show up in the debug log (the `` ` `` key)
while the game starts.

{{/if}}
To see the example item: in a colony, open the debug actions menu (the icon in the same row), choose
"Spawn thing", and pick `{{ProjectName}}_ExampleItem`.{{#if Code && Settings}} The settings window is under Options, Mod settings.{{/if}}

## When it does not load

- **The mod is not in the Mods list:** `About\About.xml` is missing, misspelled, or the mod sits one folder
  too deep (`Mods\{{ProjectName}}\{{ProjectName}}\About`). After unzipping a download this is the usual cause.
- **Listed but red, "made for an older version":** `supportedVersions` in `About.xml` does not list the
  running game version.
{{#if Code}}
- **Defs load but the code never runs (no "loaded" line):** the dll is not in `{{GameVersion}}\Assemblies`, or the
  game is another version than {{GameVersion}} and so skips that folder. For one more version, build into a
  folder with that version's name and add it to `supportedVersions`.
- **"Could not load file or assembly" or a ReflectionTypeLoadException at start:** the dll was built for the
  wrong framework. `TargetFramework` must stay `net472` for RimWorld 1.6 and older.
{{/if}}
{{#if Code && Harmony}}
- **"Could not load ... 0Harmony":** the Harmony mod is not enabled, or is below this mod in the list.
{{/if}}
- **A red "XML error" or "Could not resolve cross-reference" line:** a typo in a Def. The message names the
  Def and the tag.
- **"Duplicate def" or another mod's item changed:** two Defs share a `defName`. Keep the `{{ProjectName}}_`
  prefix on every defName.
- **The `packageId`** (`{{PackageId}}`) is how the game, saves and other mods know this mod. Do not change it
  after release.
- **RimWorld 1.7** moved to .NET 10 and needs a different project: a dll built by this one does not load
  there. The XML parts carry over.
