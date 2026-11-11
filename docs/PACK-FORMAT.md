# Template pack format

A pack is a folder (zipped into the exe at build time). Everything in it is plain text, which is why hundreds of
variations cost a few hundred KB: files are composed on the user's PC from small parts, never stored per variation.

```
packs/
  pack.json                      {"name": "...", "version": 3, "engines": ["Unity", "Unreal", ...]}
  games.json                     games the pack knows about (optional, see below)
  <template-id>/
    template.json                the manifest
    games.json                   optional, same shape as the top one, for games this template is about
    files/                       the project, exactly as it should land on disk
      {{ProjectName}}.csproj
      src/Plugin.cs
      .gitignore.tpl             a trailing .tpl is dropped (use it for names that would affect this repo)
```

`version` is a whole number. A downloaded pack replaces the built-in one only when its version is higher.
`engines` is the order of the engine filter chips.

## template.json

```json
{
  "id": "unity-bepinex5",                  must equal the folder name
  "name": "BepInEx 5 plugin",
  "engine": "Unity",                       the filter chip it sits under
  "loader": "BepInEx 5",
  "language": "C#",
  "summary": "One line shown on the card.",
  "description": "A few sentences shown above the options.",
  "docs": "https://...",
  "order": 10,                             lower sorts first inside its engine
  "eol": "crlf",                           or "lf". .sh and gradlew are always lf, .bat/.cmd always crlf
  "folder": "{{ModName}}",                 name of the folder the project is created in (this is the default)
  "main": "src/Plugin.cs",                 optional: the file the preview opens on (a template string, output path)
  "match": { "engine": "unity", "backend": "mono", "family": "", "loader": "BepInEx 5" },
  "options": [ ... ],
  "derived": { "ProjectName": "{{ModName|pascal}}" },
  "when": { "Patches": "Harmony", "*.md": "Readme" },
  "next": [ "Open the folder in your editor.", "Run: dotnet build" ]
}
```

`match` says which picked game the template suits. `engine` is `unity`, `unreal` or `godot`; `backend` is `mono`
or `il2cpp`; `family` is a game-specific tag from games.json (`beamng`, `minecraft-java`...); `loader` is the
loader name the game scanner reports when it is already installed (`BepInEx 5`, `BepInEx 6`, `MelonLoader`,
`UE4SS`). Leave `match` out for a template that fits any game.

### options

```json
{ "id": "ModName", "label": "Mod name", "type": "text", "default": "MyMod", "required": true,
  "pattern": "[A-Za-z][A-Za-z0-9 _-]*", "patternHelp": "letters, digits, spaces, - and _",
  "help": "Shown under the field.", "group": "Features", "from": "GamePath",
  "showWhen": "Harmony", "remember": true }
```

| type | value in templates |
|---|---|
| `text` | what was typed, trimmed |
| `bool` | `true` or empty. `"default": true` turns it on |
| `choice` | the picked `value`. `"choices": [{"value": "il2cpp", "label": "IL2CPP"}]` or plain strings |
| `folder` | a folder path, with a Browse button |
| `guid` | a fresh GUID, never shown. One per option, stable while the user edits |

- `from` names a game variable; when a game is picked and that variable is not empty it becomes the default. For a
  `choice`, the game value must equal a choice value, or start with one followed by a dot (`EngineVersion` `4.27`
  picks the choice `4`); anything else keeps the template's default.
- An empty file in `files/` is kept as an empty file (for marker files like `enabled.txt`).
- `showWhen` hides the option while the condition is false (it keeps its default value).
- `remember` keeps the last value across runs. Use it for author names.
- `pattern` is a regular expression the whole value must match.
- `group` puts options under a small heading. Options without one come first.

### Game variables (always defined, empty when unknown)

`HasGame`, `GameName`, `GamePath`, `GameExe` (no .exe), `GameDataDir` (Unity: `Name_Data`), `UnityVersion`
(`2022.3.10`), `UnityBackend` (`mono`/`il2cpp`), `UnityTfm` (`netstandard2.1`, `net46` or `net35`, picked from
the Unity version), `EngineVersion`, `GameArch` (`x64`/`x86`), `SteamAppId`, `UnrealProject`, `UnrealBinDir`
(`Project\Binaries\Win64`).

Built-ins: `Year`, `Date` (`2026-11-10`), `TemplateId`, `TemplateName`.

A template must produce a sensible project when no game is picked: every game variable can be empty.

### derived

Extra names computed from the others, each a template string. Order does not matter. Use them for anything that
appears in a path, because file names cannot contain `|`.

### when

Keys are paths under `files/` (a file, a folder, or a pattern with `*`); the value is a condition. A file is left
out when any rule that covers it is false. A pattern with `*` must match the whole path (`*/advancement/*`, not
`*/advancement`). Keys may contain `{{names}}`, written as they appear under `files/`. The validator fails a key
that matches no file.

## The template language

- `{{Name}}` inserts a value. `{{Name|filter}}` changes it first; filters chain.
- Block tags, each on its own line (the whole line disappears):

  ```
  {{#if Harmony && !Minimal}}
  ...
  {{#elif Backend == il2cpp}}
  ...
  {{#else}}
  ...
  {{/if}}
  ```

  Conditions: a name (true when not empty and not `false`), `!`, `==`, `!=` (the right side is a literal, quotes
  optional, compared without case), `&&`, `||`, brackets.
- A name that does not exist is an error, never an empty string. Typos fail loudly.
- Double braces that do not hold a plain name pass through untouched (`{{ 1, 2 }}`, `{{x.y}}`). If a file needs a
  literal `{{word}}` (SMAPI and Content Patcher tokens, for example), write `{{raw word}}`. Windows paths such as
  `{{GamePath}}\{{UnrealBinDir}}` are fine: a backslash before braces has no special meaning.
- Inline blocks work too (`{{#if GameName}} for {{GameName}}{{/if}}`), including inside `derived` values.

| filter | `My cool-mod 2` becomes |
|---|---|
| `pascal` | `MyCoolMod2` |
| `camel` | `myCoolMod2` |
| `snake` | `my_cool_mod_2` |
| `kebab` | `my-cool-mod-2` |
| `flat` | `mycoolmod2` |
| `lower`, `upper`, `trim`, `nospace` | as named |
| `ident` | strips everything but letters, digits and `_` |
| `json` | escaped for inside a JSON string |
| `xml` | escaped for XML text or an attribute |
| `str` | escaped for a Lua, C, C# or Java string in double quotes |
| `fwd`, `back` | slashes turned one way |

Use `|json`, `|xml` and `|str` wherever a typed value or a path lands inside a string. Game paths contain
backslashes and sometimes `&` or quotes.

## games.json

```json
{ "games": [
  { "name": "Lethal Company", "appid": "1966720", "engine": "unity", "backend": "mono",
    "family": "", "loader": "BepInEx 5", "notes": "One line of what a modder should know.",
    "folders": ["Lethal Company", "%APPDATA%\\.minecraft"] }
] }
```

Installed games are matched by Steam app id, then by name, then by install folder name. A `folders` entry with
`%` or `:` in it is a full path to look for, for games that are not in a store library. `family` ties a game to
its own templates. Only list facts you are sure of: a wrong backend sends people to the wrong template.

## Rules for template content

1. The generated project must work as generated. Someone edits one file and has a mod. No file may be missing, and
   no file may be in the wrong place. If a file must be supplied by the user (a game dll), the project still builds
   or loads without it and the README says exactly where it goes.
2. Every template has a `README.md` in its files: the folder layout and why each file is there, how to build, where
   the result goes in the game, and the common ways it silently fails to load.
3. The main source file does something visible on load (a log line at least) so the user can tell it loaded.
4. Version numbers that go stale (loader versions, game versions) are options with a default, not hard-coded.
5. No long dashes (em or en) anywhere. No mention of how the template was written.
6. Check it: `ModForge.exe --cli validate --packs <packs folder> <id>` renders every option combination, with and
   without a game, and parses every .json and XML file that comes out.
