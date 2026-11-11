# {{ModName}}

A resource pack for Minecraft: Java Edition {{MinecraftVersion}}.

There is nothing to build. This folder is the pack.

## Layout

| File | What it is for |
|---|---|
| `pack.mcmeta` | Makes the folder a resource pack and says which game version it is for. Must be directly inside the pack folder. |
{{#if Lang}}
| `assets/minecraft/lang/en_us.json` | Replaces English texts. Only the keys listed here change; it renames the Singleplayer button. |
{{/if}}
{{#if Model}}
| `assets/minecraft/models/item/stick.json` | Replaces the stick's model. It points at the blaze rod texture, so sticks look like blaze rods. |
{{/if}}
| `assets/minecraft/textures/readme.txt` | Where pictures go and how they are named. |

`assets/minecraft` holds files that replace the game's own. A file only replaces the game's file when its path
and name are exactly the same.

To give the pack a picture in the pack list, put a square PNG named `pack.png` next to `pack.mcmeta`.

## Install

Copy the whole `{{PackFolder}}` folder to:

```
{{#if GamePath}}{{GamePath}}{{#else}}%APPDATA%\.minecraft{{/if}}\resourcepacks\{{PackFolder}}\pack.mcmeta
```

In the game: Options, Resource Packs, move the pack to the Selected side, Done. A zip works too, as long as
`pack.mcmeta` is at the top level of the zip and not inside a folder in it.

## Check that it loaded

{{#if Lang}}
- With the game language set to English (US), the title screen button reads `Singleplayer ({{ModName}})`.
{{/if}}
{{#if Model}}
- A stick in the inventory looks like a blaze rod.
{{/if}}
- The pack is on the Selected side of the resource pack list, without a red "made for another version" note.
- F3+T reloads all packs after you change a file.

## When it does not load

- **The pack is not in the list:** `pack.mcmeta` is not directly inside the pack folder (a common cause is a
  folder inside a folder after unzipping), or it is not valid JSON.
- **The pack is red, "made for a newer/older version":** the format number does not match the game. It still
  works if you accept the warning, but files may be in places the game no longer looks.
{{#if MinecraftVersion == 1.21.1}}
  This pack says `"pack_format": 34`, which is Minecraft 1.21 and 1.21.1.
{{#else}}
  This pack's `min_format` and `max_format` are the number for Minecraft {{MinecraftVersion}}. To allow newer
  versions too, raise `max_format`.
{{/if}}
  Resource pack formats: 1.21 and 1.21.1 are 34, 1.21.11 is 75, 26.1 is 84, 26.2 is 88, 26.3 is 97. These are
  not the data pack numbers.
- **`pack_format` or `min_format`:** from 1.21.9 on (resource format 65 and up) `pack.mcmeta` uses `min_format`
  and `max_format`. Older versions use a single `pack_format`.
- **A picture or model does not change:** the path or name differs from the game's. Names are lower case with
  underscores; `Stone.png` or `textures/blocks/stone.png` (old plural folder) is ignored without an error.
- **A text does not change:** the lang file is only used for its own language. `en_us.json` does nothing while
  the game is set to another language.
- **An item is a purple and black square:** its model names a texture that does not exist. Check the path after
  `layer0`: `minecraft:item/blaze_rod` means `assets/minecraft/textures/item/blaze_rod.png`.
- **Changing how an item looks in 1.21.4 and newer:** which model an item uses is decided by
  `assets/minecraft/items/<item>.json`. Replacing `models/item/<item>.json`, as this pack does, still works
  because the game's own items file points at that model.
