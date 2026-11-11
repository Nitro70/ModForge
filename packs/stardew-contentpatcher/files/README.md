# {{ModName}}

A Content Patcher pack for Stardew Valley. It has no code: Content Patcher reads the JSON files in this folder
and changes the game's data, images and maps while the game runs. The game's own files are never touched.

## Layout

This folder is the mod. Its name does not matter to the game; `[CP]` in front is the convention for Content
Patcher packs.

| File | What it is for |
|---|---|
| `manifest.json` | What SMAPI reads first. `ContentPackFor` hands the folder to Content Patcher. Without this file, or with it one folder too deep, the pack is ignored. |
| `content.json` | The changes. `Format` is the Content Patcher version the pack was written for, `Changes` is the list of edits. The file name is fixed. |
{{#if I18n}}
| `i18n/default.json` | The English text, used in `content.json` as `{{i18n: key}}`. A translation is another file beside it, such as `i18n/de.json`, with the same keys. |
{{/if}}
| `README.md` | This file. It is not needed in the game. |

Images, maps and extra JSON files go into a folder named `assets` beside `content.json` (the usual name, any
name works) and are referred to as `assets/yourfile.png`.

## Install

Stardew Valley needs [SMAPI](https://smapi.io) and
[Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) {{Format}} or newer.

Put this whole folder into the game's `Mods` folder, so that the result is:

```
{{#if GamePath}}
{{GamePath}}\Mods\[CP] {{ModName}}\manifest.json
{{GamePath}}\Mods\[CP] {{ModName}}\content.json
{{#else}}
<game folder>\Mods\[CP] {{ModName}}\manifest.json
<game folder>\Mods\[CP] {{ModName}}\content.json
{{/if}}
```

The game folder is the one with `Stardew Valley.exe`. Working directly in the `Mods` folder is the easy way:
edit, then restart the game (or type `patch reload {{UniqueId}}` in the SMAPI console).

## Check that it loaded

Start the game through SMAPI. The SMAPI console lists the pack under "Loaded 1 content packs" (or more):

```
{{ModName}} {{Version}} by {{Author}} | for Content Patcher | {{Description}}
```

Load a save and hover over a parsnip. Its description is now "A spring tuber, changed by {{ModName}}."
Type `patch summary {{UniqueId}}` in the SMAPI console to see every change of this pack and whether it applied.
{{#if Config}}

After the first run a `config.json` appears in this folder with the `ChangeParsnip` setting. Players with
Generic Mod Config Menu also get it in the in-game menu.
{{/if}}

## The example change

`content.json` has one `EditData` change. It edits the `Description` field of entry `24` (the parsnip) in the
game's `Data/Objects` asset.{{#if Config}} The `When` block applies it only while the `ChangeParsnip` setting is true.{{/if}}

The other actions follow the same shape. To replace an image, add the png and a `Load` change:

```json
{
  "Action": "Load",
  "Target": "Portraits/Abigail",
  "FromFile": "assets/abigail.png"
}
```

`EditImage` pastes into part of an image, `EditMap` changes a map, `Include` reads more changes from another
file. The author guide linked at the bottom covers each one.

## When it does not load

- **The pack is not in SMAPI's list:** `manifest.json` is not directly inside the folder under `Mods`
  (a folder inside a folder is the usual cause after unzipping), or the file got renamed.
- **SMAPI skips the pack because Content Patcher is not installed:** install Content Patcher.
- **SMAPI skips the pack because Content Patcher is too old:** update Content Patcher, or lower `Format` in
  `content.json` and `MinimumVersion` in `manifest.json` together.
- **"content.json is invalid" or a JSON error with a line number:** a missing comma, quote or bracket. Check
  that line and the one before it.
- **Listed, but nothing changes in game:** run `patch summary {{UniqueId}}` in the SMAPI console. It shows for
  each change whether it loaded, whether its conditions matched, and whether it was applied. A wrong `Target`
  name is the usual cause: asset names have no `Content\` in front and no file extension.
- **The `UniqueID`** (`{{UniqueId}}`) is how SMAPI and other mods know this pack. Do not change it after
  release.

## More

- Author guide: https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md
- What every game asset contains: https://stardewvalleywiki.com/Modding:Index
