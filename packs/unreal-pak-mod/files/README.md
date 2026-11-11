# {{ModName}}

A pak mod{{#if GameName}} for {{GameName}}{{/if}} (Unreal Engine {{EngineVersion}}): files that replace or add to the game's
content, packed into `{{PakName}}.pak`. No mod loader is involved; the engine mounts the pak itself.

## Layout

| File | What it is for |
|---|---|
| `{{PakName}}\` | The contents of the pak. Its name is the pak's name. |
| `{{PakName}}\{{UnrealProject}}\Content\` | Stands for the game's `{{UnrealProject}}\Content` folder (`/Game/` in Unreal). Your files go here, at the same path they have in the game. |
| `pack.bat`, `pack.ps1` | Pack `{{PakName}}` into `dist\` and copy the result into the game. |
| `tools\` | Where `pack.bat` looks for `UnrealPak.exe`{{#if Format == iostore}}, `repak.exe` and `retoc.exe`{{#else}} or `repak.exe`{{/if}}. |
| `game-path.txt` | The game's top folder. One line. |
| `dist\` | Created by `pack.bat`: the finished mod. |

## The three things that must be exact

1. **The folder depth.** Inside the pak every path starts at the game's top folder:
   `{{UnrealProject}}/Content/...`. So the first folder under `{{PakName}}` must be `{{UnrealProject}}`, then
   `Content`. One level off in either direction and the game loads the pak but finds nothing in it.

   ```
   {{PakName}}\
       {{UnrealProject}}\
           Content\
               Characters\Hero\T_Hero_D.uasset      replaces /Game/Characters/Hero/T_Hero_D
               Characters\Hero\T_Hero_D.uexp
   ```

2. **The `_P` at the end of the name.** `{{PakName}}.pak` ends in `_P`, which makes the engine treat it as a
   patch: its files win over the same files in the game's own paks. Without `_P` the game's originals win and
   nothing changes. Between two `_P` mods that change the same file, the name that sorts later wins; put a
   `z` in front (`z{{PakName}}`: rename the folder) to load after another mod.

3. **Where the pak goes.**

   ```
{{#if GamePath}}
   {{GamePath}}\{{UnrealProject}}\Content\Paks\{{ModsFolder}}\{{PakName}}.pak
{{#else}}
   <Game>\{{UnrealProject}}\Content\Paks\{{ModsFolder}}\{{PakName}}.pak
{{/if}}
   ```

   The engine mounts every pak under `Content\Paks`, sub-folders included. `{{ModsFolder}}` keeps mods apart from
   the game's files (the `~` of `~mods` makes it sort last). Create the folder if it is not there (`pack.bat` does).

## Getting the files

The files must be **cooked** assets for Unreal Engine {{EngineVersion}}: `.uasset` with its `.uexp` (and `.ubulk`
when there is one). Source files such as `.png` or `.fbx` do nothing in a pak.

- Make them in the Unreal Editor of the same engine version: put the asset at the same `/Game/...` path and
  with the same name as the one it replaces, then cook (File, Cook Content for Windows). The cooked files are
  under `Saved\Cooked\WindowsNoEditor\<YourProject>\Content\` (UE4) or `Saved\Cooked\Windows\<YourProject>\Content\`
  (UE5). Copy only the files you changed.
- Or extract the original from the game (FModel, UModel, `repak unpack`{{#if Format == iostore}}, `retoc to-legacy`{{/if}}), change it with a
  tool that writes cooked assets (UAssetGUI for data, a texture injector for textures) and put the result here.

Loose files that are not assets (`.ini`, `.json`, `.txt` the game reads from `Content`) go in the same way.

## Pack and install

1. Put a packer into `tools\` (see `tools\README.txt`).
{{#if GamePath}}
2. Run `pack.bat`.
{{#else}}
2. Write the game's top folder on the first line of `game-path.txt`, then run `pack.bat`.
   `pack.bat -NoInstall` only builds into `dist\`.
{{/if}}

{{#if Format == iostore}}
What it does for an IoStore game:

```
UnrealPak.exe "dist\legacy\{{PakName}}.pak" -create="dist\filelist.txt"      (or: repak pack)
retoc.exe to-zen "dist\legacy\{{PakName}}.pak" "dist\{{PakName}}.utoc" --version UE<major>_<minor>
```

`retoc to-zen` writes three files, `{{PakName}}.utoc`, `{{PakName}}.ucas` and `{{PakName}}.pak`. All three go into
`Content\Paks\{{ModsFolder}}` together; one without the others is ignored.
{{#else}}
What it does:

```
UnrealPak.exe "dist\{{PakName}}.pak" -create="dist\filelist.txt"
```

`filelist.txt` is UnrealPak's response file, written by the script with one line per file:

```
"C:\...\{{PakName}}\{{UnrealProject}}\Content\Some\File.uasset" "../../../{{UnrealProject}}/Content/Some/File.uasset"
```

The second path is where the file sits inside the pak; `../../../` is the game's top folder. With `repak.exe`
instead the script runs `repak pack --version <V> <folder> <pak>`, which uses the same `../../../` mount point.
Add `-compress` to the UnrealPak line in `pack.ps1` for a smaller pak.
{{/if}}

## When it does not load

- **Nothing changes in the game:**
  - the pak name does not end in `_P`, so the original files win;
  - the paths inside the pak are wrong. List them (`repak list dist\{{PakName}}.pak`): every line must start
    with `{{UnrealProject}}/Content/`, not with `Content/`, `{{PakName}}/` or a drive letter;
  - the asset is not at exactly the path and name of the original;
  - the file is not cooked, or was cooked with another engine version than {{EngineVersion}}.
{{#if Format == iostore}}
- **The game uses IoStore and only the `.pak` was copied:** the `.utoc` and `.ucas` must sit next to it, with
  the same name.
- **`retoc` refuses the version:** it wants the game's engine version as `UE5_3`, `UE5_4` and so on. Change
  the engine version at the top of `pack.ps1`.
{{#else}}
- **The game has `.utoc` and `.ucas` files in `Content\Paks`:** it uses IoStore and ignores assets in a plain
  pak. Generate this project again with the IoStore format (`pack.bat` warns about this).
{{/if}}
- **The game has `.sig` files next to its paks and ignores the mod:** it checks pak signatures. Some games
  accept a copy of any original `.sig` renamed to `{{PakName}}.sig`; others need a signature bypass made for
  that game. Look in the game's modding community.
- **The game crashes while loading:** an asset in the pak does not match the engine version or the original's
  class. Remove files from the pak one at a time to find it.
- **The game's paks are encrypted:** that only matters for reading the originals (the tools then need the
  game's AES key). An unencrypted mod pak still loads.
- **It worked until another mod was added:** both change the same file and the other one sorts later. Rename
  this one so it sorts after it.
