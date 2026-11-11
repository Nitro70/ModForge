# {{ModName}}

A data pack for Minecraft: Java Edition {{MinecraftVersion}}. Namespace: `{{Namespace}}`.

There is nothing to build. This folder is the pack.

## Layout

| File | What it is for |
|---|---|
| `pack.mcmeta` | Makes the folder a data pack and says which game version it is for. Must be directly inside the pack folder. |
| `data/minecraft/tags/function/load.json` | Lists the functions the game runs once on load. It names `{{Namespace}}:load`. |
| `data/minecraft/tags/function/tick.json` | Lists the functions the game runs every tick. It names `{{Namespace}}:tick`. |
| `data/{{Namespace}}/function/load.mcfunction` | Runs on load and after `/reload`. Prints a message. |
| `data/{{Namespace}}/function/tick.mcfunction` | Runs every tick. Empty apart from comments. |
| `data/{{Namespace}}/function/hello.mcfunction` | A function to run by hand: `/function {{Namespace}}:hello`. |
{{#if Advancement}}
| `data/{{Namespace}}/advancement/got_dirt.json` | A hidden advancement: the first time a player has dirt in the inventory it runs `{{Namespace}}:hello`. |
{{/if}}
{{#if Recipe}}
| `data/{{Namespace}}/recipe/dirt_to_diamond.json` | A crafting recipe: one dirt anywhere in the grid makes a diamond. |
{{/if}}
{{#if LootTable}}
| `data/{{Namespace}}/loot_table/example.json` | A loot table. Roll it with `/loot give @s loot {{Namespace}}:example`. |
{{/if}}

The two tag files are under `data/minecraft` on purpose: `load` and `tick` are the game's own tags. Everything of
your own goes under `data/{{Namespace}}`.

## Install

Copy the whole `{{PackFolder}}` folder into the world's `datapacks` folder:

```
{{#if GamePath}}{{GamePath}}{{#else}}%APPDATA%\.minecraft{{/if}}\saves\<world name>\datapacks\{{PackFolder}}\pack.mcmeta
```

On a server the folder is `<server>\<level-name>\datapacks`. A zip works too, as long as `pack.mcmeta` is at the
top level of the zip and not inside a folder in it.

Then open the world. In a world that is already open, run `/reload`.

## Check that it loaded

- `/datapack list` shows `[file/{{PackFolder}}]` among the enabled packs.
- After `/reload`, chat shows `{{ModName}} loaded`.
- `/function {{Namespace}}:hello` prints a greeting. The command needs cheats on, or operator rights on a server.

## When it does not load

The game does not report most of these. The pack or the file is just skipped.

- **The pack is not in `/datapack list` at all:** `pack.mcmeta` is not directly inside the pack folder (a common
  cause is a folder inside a folder after unzipping), or it is not valid JSON.
- **The pack is listed in red as made for another version:** the format number does not match the game.
{{#if MinecraftVersion == 1.21.1}}
  This pack says `"pack_format": 48`, which is Minecraft 1.21 and 1.21.1.
{{#else}}
  This pack's `min_format` and `max_format` are the number for Minecraft {{MinecraftVersion}}. To allow newer
  versions too, raise `max_format`.
{{/if}}
  Format numbers: 1.21 and 1.21.1 are 48, 1.21.11 is 94, 26.1 is 101, 26.2 is 107, 26.3 is 121.
- **`pack_format` or `min_format`:** from 1.21.9 on (format 82 and up) `pack.mcmeta` uses `min_format` and
  `max_format`. Older versions use a single `pack_format`. Using the wrong one makes the pack invalid.
- **Functions never run:** the folders are singular since 1.21: `function`, `advancement`, `recipe`, `loot_table`,
  `predicate`, `tags/function`. Older guides say `functions` and `tags/functions`. Those names load without any
  error and are ignored.
- **`Unknown function {{Namespace}}:hello`:** file and folder names may only use lower case letters, digits, `_`,
  `-` and `.`. One capital letter or space and the file is skipped. The file must end in `.mcfunction`.
- **A function is cut off or missing:** one bad command makes the game drop the whole function file. The reason
  is in `logs/latest.log`, not in chat. Commands in a function have no leading `/`.
- **The load message does not appear when opening the world:** the load function runs before you have joined.
  Run `/reload` to see it.
{{#if Recipe}}
- **The recipe does not exist:** recipe JSON changed shape over time.
{{#if MinecraftVersion == 1.21.1}}
  In 1.21 and 1.21.1 an ingredient is an object, `{"item": "minecraft:dirt"}`, and the result uses `"id"`.
  From 1.21.2 on ingredients are plain strings.
{{#else}}
  In this version an ingredient is a plain string, `"minecraft:dirt"`, and the result uses `"id"`. The older
  object form `{"item": ...}` is rejected.
{{/if}}
{{/if}}
