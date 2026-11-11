# {{ModName}}

A Blueprint logic mod{{#if GameName}} for {{GameName}}{{/if}} (Unreal Engine {{EngineVersion}}), loaded by UE4SS's BPModLoaderMod.

The mod itself is a Blueprint you make in the Unreal Editor. It cannot be generated as text, so this project
holds everything around it: the folder tree with the names that must match, the packing script, and the
steps for the editor.

## How the loader finds the mod

After every map load, BPModLoaderMod goes through `{{UnrealProject}}\Content\Paks\LogicMods`. For each
`<Name>.pak` it loads the asset `/Game/Mods/<Name>/ModActor` and spawns it into the world. That gives three
names that must agree, and one that is fixed:

| What | Must be |
|---|---|
| The pak file | `{{PakName}}.pak` |
| The folder in the editor project | `Content\Mods\{{PakName}}` |
| The folder inside the pak | `{{UnrealProject}}\Content\Mods\{{PakName}}` |
| The Blueprint | `ModActor` (an Actor Blueprint; its class is then `ModActor_C`) |

If any of them differs the loader logs that the mod's class is not valid and moves on.

## Layout

| File | What it is for |
|---|---|
| `{{PakName}}\` | The contents of the pak. Its name is the pak's name. |
| `{{PakName}}\{{UnrealProject}}\Content\Mods\{{PakName}}\` | Where the cooked `ModActor.uasset` and `ModActor.uexp` go. |
| `pack.bat`, `pack.ps1` | Check the layout, pack into `dist\`, copy into the game's `LogicMods` folder. |
| `tools\` | Where `pack.bat` looks for `UnrealPak.exe`{{#if Format == iostore}}, `repak.exe` and `retoc.exe`{{#else}} or `repak.exe`{{/if}}. |
| `game-path.txt` | The game's top folder. One line. |

## Make the Blueprint

1. Install Unreal Engine {{EngineVersion}} (the game's version; a different one makes assets the game cannot
   load) and create an empty Blueprint project. Its name does not matter.
2. In the Content Browser create the folder `Mods`, and inside it `{{PakName}}`. The Blueprint's path must come
   out as `/Game/Mods/{{PakName}}/ModActor`.
3. In that folder: Add, Blueprint Class, **Actor**. Name it exactly `ModActor`.
4. Open it. The loader calls two optional events by name. Add them as Custom Events with exactly these
   names and no inputs:
   - `PreBeginPlay`: called right after the actor is spawned.
   - `PostBeginPlay`: called after the actor's BeginPlay.

   The usual `Event BeginPlay` and `Event Tick` work too.
5. Put your logic behind those events. Keep everything the mod needs (widgets, textures, other Blueprints)
   inside `Content\Mods\{{PakName}}`, so it lands in the pak with the right paths.
6. Cook: in UE4 File, Cook Content for Windows; in UE5 Platforms, Windows, Cook Content.
7. Copy the cooked files of your mod folder

   ```
   <EditorProject>\Saved\Cooked\WindowsNoEditor\<EditorProject>\Content\Mods\{{PakName}}\      (UE4)
   <EditorProject>\Saved\Cooked\Windows\<EditorProject>\Content\Mods\{{PakName}}\              (UE5)
   ```

   into

   ```
   {{PakName}}\{{UnrealProject}}\Content\Mods\{{PakName}}\
   ```

   in this project. The editor project's name is replaced by the game's (`{{UnrealProject}}`) on the way: that
   is the folder the game calls `/Game/`.

To use the game's own classes in the Blueprint (its player character, its widgets) the editor project needs
stand-ins for them at the same paths with the same names. UE4SS can generate those headers (the UHT dump);
that is a topic of its own and not needed for a first mod.

## Pack and install

1. Put a packer into `tools\` (see `tools\README.txt`).
{{#if GamePath}}
2. Run `pack.bat`.
{{#else}}
2. Write the game's top folder on the first line of `game-path.txt`, then run `pack.bat`.
{{/if}}

`pack.bat` stops if `ModActor.uasset` is not where the loader will look. Otherwise it builds
{{#if Format == iostore}}
`dist\{{PakName}}.pak`, `dist\{{PakName}}.utoc` and `dist\{{PakName}}.ucas` and copies all three to:
{{#else}}
`dist\{{PakName}}.pak` and copies it to:
{{/if}}

```
{{#if GamePath}}
{{GamePath}}\{{UnrealProject}}\Content\Paks\LogicMods\
{{#else}}
<Game>\{{UnrealProject}}\Content\Paks\LogicMods\
{{/if}}
```

Unlike an asset replacement mod, the name has no `_P`: nothing is being overridden, and the loader takes the
mod's name from the file name.

## Check that it loaded

UE4SS must be installed, with `BPModLoaderMod : 1` in its `Mods\mods.txt` (it is on in the stock file). Load
into a map, then look in `UE4SS.log`:

```
[Lua] Loading mod: {{PakName}}
[Lua] Actor: ModActor_C /Game/...
```

The second line is the spawned actor. `Print String` shows nothing in most released games, so do not rely on
it to tell whether the mod runs.

## When it does not load

- **No "Loading mod: {{PakName}}" line:** the pak is not directly in `Content\Paks\LogicMods` (a sub-folder does
  not count), or BPModLoaderMod is off, or UE4SS is not running (no `UE4SS.log`).
- **"ModClass for '{{PakName}}' is not valid":** the loader found the pak but not the asset
  `/Game/Mods/{{PakName}}/ModActor`. One of the names in the table at the top is off, the pak's paths do not
  start with `{{UnrealProject}}/Content/` (list them with `repak list dist\{{PakName}}.pak`), or the Blueprint was
  cooked with another engine version.
{{#if Format == iostore}}
- **IoStore:** the `.utoc` and `.ucas` must sit next to the `.pak` in `LogicMods`, with the same name.
{{#else}}
- **The game has `.utoc` files in `Content\Paks`:** it uses IoStore and cannot read assets from a plain pak.
  Generate this project again with the IoStore format.
{{/if}}
- **The actor spawns but nothing happens:** the events are not named exactly `PreBeginPlay` and
  `PostBeginPlay`, or the logic depends on something that does not exist yet in the main menu map. The actor
  is spawned again in every map.
- **The game has `.sig` files next to its paks:** it checks pak signatures and may refuse the mod. See the
  game's modding community for its signature bypass.
- **Load order matters between two logic mods:** list them, one name per line without `.pak`, in
  `Mods\BPModLoaderMod\load_order.txt`.
