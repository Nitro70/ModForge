# {{ModName}}

An add-on for Minecraft: Bedrock Edition, made of two packs that belong together.

There is nothing to build. The two folders are the packs.

## Layout

| File | What it is for |
|---|---|
| `{{BpFolder}}/manifest.json` | Makes the folder a behavior pack. Holds the pack's UUID, its modules, and the dependency on the resource pack{{#if Script}} and on the scripting API{{/if}}. |
{{#if Script}}
| `{{BpFolder}}/scripts/main.js` | The script. The manifest names it as `"entry"`. It greets each player who joins. |
{{/if}}
{{#if Function}}
| `{{BpFolder}}/functions/{{FunctionName}}.mcfunction` | A function: `/function {{FunctionName}}`. |
{{/if}}
| `{{RpFolder}}/manifest.json` | Makes the folder a resource pack. Its header UUID is the one the behavior pack depends on. |
{{#if Lang}}
| `{{RpFolder}}/texts/en_US.lang` | Replaces English texts. It renames the stick. |
{{/if}}

Behavior packs hold what things do (functions, scripts, items, entities, loot tables, recipes). Resource packs
hold what things look and sound like (textures, models, texts, sounds). In Bedrock the folder names inside a pack
are fixed: `functions`, `scripts`, `items`, `entities`, `loot_tables`, `recipes` in a behavior pack and `textures`,
`texts`, `models`, `sounds` in a resource pack.

## Install

Copy each pack folder into its development folder:

```
{{#if GamePath}}{{GamePath}}{{#else}}%APPDATA%\Minecraft Bedrock{{/if}}\Users\Shared\games\com.mojang\development_behavior_packs\{{BpFolder}}\manifest.json
{{#if GamePath}}{{GamePath}}{{#else}}%APPDATA%\Minecraft Bedrock{{/if}}\Users\Shared\games\com.mojang\development_resource_packs\{{RpFolder}}\manifest.json
```

The game reads the development folders again every time it starts, so you can edit the files in place.

Then, in Minecraft: Create New World (or edit a world), open Behavior Packs, find `{{ModName}}` under Available
and activate it. The resource pack is activated with it because of the dependency.

## Check that it loaded

{{#if Script}}
- When you enter the world, chat shows `Hello from {{ModName}}!`.
{{/if}}
{{#if Function}}
- With cheats on, `/function {{FunctionName}}` says hello.
{{/if}}
{{#if Lang}}
- With the game language set to English (US), a stick is named `Stick ({{ModName}})`.
{{/if}}
- Turn on the content log to see errors: Settings, Creator, Enable Content Log GUI and Enable Content Log File.
  Almost every problem below shows a line there.

After editing a script or a function, run `/reload` in the world. After editing a manifest, restart the game.

## When it does not load

- **The pack is not in the list at all:** `manifest.json` is not directly inside the pack folder (a folder inside
  a folder), it is not valid JSON, or the pack is in the wrong development folder (a behavior pack in
  `development_resource_packs` or the other way round).
- **The pack shows up twice, or an old version keeps loading:** two packs have the same UUID. Every pack header
  and every module needs its own UUID; never copy a manifest without replacing all of them. The UUIDs in this
  project were made for it and are already unique.
- **"Missing dependency" on the behavior pack:** the `uuid` and `version` under `dependencies` must be the same as
  the `uuid` and `version` in the header of `{{RpFolder}}/manifest.json`. Change the resource pack's version and
  you must change it in both files.
- **The game says the pack needs a newer version:** `min_engine_version` ({{MinEngineVersion}}) is newer than your
  game. Lower it in both manifests, or update the game.
{{#if Script}}
- **The script does nothing:** the content log says why. The usual causes: the game does not have
  `@minecraft/server` {{ScriptApiVersion}} (each game version supports stable versions up to its own; lower the
  number in the manifest), `"entry"` does not point at `scripts/main.js`, or the script has a syntax error.
- **`worldLoad` is undefined:** that event exists from `@minecraft/server` 2.0.0. With a 1.x version it is called
  `worldInitialize`.
- **A version with `-beta` in it:** needs the Beta APIs experiment turned on in the world. Stable versions like
  {{ScriptApiVersion}} do not.
- **Errors about "early execution":** code at the top level of a script runs before the world exists. Do the
  work inside an event handler instead.
{{/if}}
{{#if Function}}
- **`/function` does not find the function:** the folder must be named `functions` (plural, unlike current Java
  data packs) and sit directly in the behavior pack. Run `/reload` after adding a file.
{{/if}}
- **Changes do not show up:** the world keeps its own copy of packs that were added from the normal `behavior_packs`
  folder. Packs in the development folders are read fresh each start, which is why they go there.

## Version numbers

`"version": [1, 0, 0]` in the header is the pack's own version. Raise it when you hand out a new build, in the
resource pack header and in the behavior pack's dependency on it together.

## Sharing it

Zip the contents of each pack folder and rename the zips to `.mcpack`, or zip both pack folders together and
rename that to `.mcaddon`. Opening such a file imports it into the game.
