# {{ModName}}

A Terraria mod for tModLoader.

## Where this folder has to be

```
Documents\My Games\Terraria\tModLoader\ModSources\{{ProjectName}}
```

Not in the Terraria or tModLoader install folder, and not anywhere else. tModLoader only lists mods that are
folders directly inside `ModSources`, and the project imports `..\tModLoader.targets`, a file tModLoader keeps
in `ModSources`. If `ModSources` does not exist yet, start tModLoader once and open **Workshop, Develop Mods**;
the **Open Sources** button there opens the folder.

The folder name is the mod's internal name. It must stay `{{ProjectName}}`: the csproj, the namespace and the
localization file all carry the same name.

## Layout

| File | What it is for |
|---|---|
| `build.txt` | The mod's display name, author and version. The build fails without it. |
| `description.txt` | The text shown in the mod list. |
| `{{ProjectName}}.csproj` | The project. It must be named after the folder and import `..\tModLoader.targets`, which supplies the target framework, the reference to tModLoader and the packing step. |
| `{{ProjectName}}.cs` | The `Mod` class, the entry point. Its namespace must be `{{Namespace}}`. |
{{#if Item}}
| `Content/Items/{{ItemClass}}.cs` | An example item. tModLoader finds every `ModItem` class by itself. |
{{/if}}
| `Localization/en-US_Mods.{{ProjectName}}.hjson` | The English names and tooltips. The file name decides where its keys land, so keep it. |
| `Properties/launchSettings.json` | Lets Visual Studio and Rider start tModLoader with the debugger attached. |

Pictures sit next to the class that uses them, with the same name: `Content/Items/MyItem.cs` and
`Content/Items/MyItem.png`. tModLoader looks them up by namespace and class name, so a class in the wrong
namespace does not find its picture. Optional extras in the mod folder: `icon.png` (80x80, shown in the mod
list) and `description_workshop.txt`.

## Build

You need tModLoader (free on Steam, next to Terraria) and the .NET 8 SDK.

In tModLoader: **Workshop, Develop Mods**, then **Build + Reload** next to {{ProjectName}}. That compiles the
mod, packs it into `{{ProjectName}}.tmod` in `Documents\My Games\Terraria\tModLoader\Mods`, enables it and reloads.

From an editor or the command line, `dotnet build` in this folder does the compile and pack steps. After
that, enable the mod under **Workshop, Manage Mods** and reload.

## Check that it loaded

The mod is in the Mods list, and `tModLoader-Logs\client.log` in the tModLoader install folder has the line
`{{ModName}} {{Version}} loaded`.{{#if Item}} In a world, stand at a work bench with 10 dirt blocks: "{{ItemName}}" is in
the crafting list.{{/if}}

## When it does not work

- **The mod is not under Develop Mods:** the folder is not directly inside `ModSources` (one folder too deep
  is the usual cause), or its name starts with a dot.
- **The build fails on `build.txt`:** the file is missing, renamed, or has a line tModLoader cannot read.
- **The build stops with "This folder must be inside tModLoader's ModSources folder", or the editor cannot
  load the project:** same cause. `tModLoader.targets` must be one level above this folder.
- **Develop Mods says the csproj needs an upgrade, or resets it:** the csproj is not named `{{ProjectName}}.csproj`
  or lost its `<Import Project="..\tModLoader.targets" />` line. The old file is kept as `.csproj.bak`.
- **Pictures or names go missing after renaming the mod:** the folder, the csproj, the root namespace and the
  `Mods.{{ProjectName}}` part of the localization file name must all be the same word.
- **The mod fails to load with a missing texture error:** an item, tile or NPC class has no png with the same
  name next to it and no `Texture` override.
- **Item names show as `Mods.{{ProjectName}}.Items...`:** the hjson file was renamed or moved out of
  `Localization`, or the key does not match the class name.
- **Develop Mods asks for the .NET SDK:** install the .NET 8 SDK it links to, then restart tModLoader.
- **The version** in `build.txt` must be numbers and dots (`1.0.0`). Anything else is ignored and the mod
  builds as 1.0.
