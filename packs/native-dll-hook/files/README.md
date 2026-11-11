# {{ModName}}

A native C++ mod{{#if GameName}} for {{GameName}}{{/if}}: a {{#if Arch == x64}}64-bit{{#else}}32-bit{{/if}} dll that hooks game functions with MinHook.
{{#if Mode == proxy}}
It is built as `version.dll`, a proxy: the game loads it by itself, no injector needed.
{{#elif Mode == asi}}
It is built as `{{ProjectName}}.asi`, a plugin for an ASI loader.
{{#else}}
It is built as `{{ProjectName}}.dll`, to be loaded with an injector.
{{/if}}

## Layout

| File | What it is for |
|---|---|
| `CMakeLists.txt` | The build: fetches MinHook, builds the dll with a static C runtime, checks the bitness{{#if Deploy && GamePath && Mode != inject}}, copies the result into the game{{/if}}. |
| `build.bat` | Configure and build Release. |
| `src\dllmain.cpp` | The entry point. `DllMain` starts a thread; the thread opens the log, places the hooks and waits for the unload key. |
| `src\hooks.cpp` | The hooks. One example, found by byte pattern. This is the file you edit. |
| `src\scan.cpp` | The pattern scanner (`scan::FindPattern`). |
| `src\log.cpp` | The log file{{#if Console}} and the console window{{/if}} (`modlog::Write`). |
{{#if Mode == proxy}}
| `src\proxy_exports.cpp` | Forwards all 17 `version.dll` exports to the real file in the Windows folder. Without it the game would not start. |
{{/if}}

## Build

You need Visual Studio with "Desktop development with C++" (it brings CMake) and Git (CMake downloads
MinHook `{{MinHookTag}}` with it at the first configure).

```
build.bat
```

or by hand:

```
cmake -S . -B build -A {{#if Arch == x64}}x64{{#else}}Win32{{/if}}
cmake --build build --config Release
```

The result is `build\Release\{{#if Mode == proxy}}version.dll{{#elif Mode == asi}}{{ProjectName}}.asi{{#else}}{{ProjectName}}.dll{{/if}}`. Run `build.bat` from a "Developer Command Prompt for VS" if `cmake` is not
found, or open the folder in Visual Studio, which reads `CMakeLists.txt` directly.

## Install

{{#if Mode == proxy}}
Copy `build\Release\version.dll` into the folder of the exe that really runs the game:

```
{{#if GamePath && UnrealBinDir}}
{{GamePath}}\{{UnrealBinDir}}\version.dll
{{#elif GamePath}}
{{GamePath}}\version.dll
{{#else}}
<Game>\version.dll                                 most games
<Game>\<Project>\Binaries\Win64\version.dll        Unreal Engine games (next to the -Shipping.exe)
{{/if}}
```

{{#if Deploy && GamePath}}
The build copies it there after every successful build. Close the game first: it keeps the file open.
{{/if}}
Windows looks for `version.dll` in the exe's own folder before the system folder, so the game picks this one
up. To remove the mod, delete the file.
{{#elif Mode == asi}}
The game needs an ASI loader (for example Ultimate ASI Loader). Copy `build\Release\{{ProjectName}}.asi` to
where that loader looks, usually the game's folder or its `scripts` or `plugins` sub-folder:

```
{{#if GamePath && UnrealBinDir}}
{{GamePath}}\{{UnrealBinDir}}\{{ProjectName}}.asi
{{#elif GamePath}}
{{GamePath}}\{{ProjectName}}.asi
{{#else}}
<Game>\{{ProjectName}}.asi
{{/if}}
```

{{#if Deploy && GamePath}}
The build copies it there after every successful build. Close the game first: it keeps the file open.
{{/if}}
{{#else}}
Start the game, then load `build\Release\{{ProjectName}}.dll` into it with a dll injector. The dll and the
injector must both match the game: {{#if Arch == x64}}64-bit{{#else}}32-bit{{/if}}.
{{/if}}

## Check that it loaded

{{#if Console}}
A console window opens with the game and shows:
{{#else}}
A log file appears next to the dll:
{{/if}}

```
[12:00:00.000] {{ProjectName}} {{Version}} loaded into <the game's exe>
[12:00:00.001] example hook: no pattern yet. Put one into kExamplePattern in src/hooks.cpp.
[12:00:00.001] ready. Press {{UnloadKey}} in the game to remove the hooks.
```

The same lines are in `{{ProjectName}}.log` next to the dll. Press {{UnloadKey}} with the game window in front to remove the
hooks{{#if Mode == proxy}}{{#else}} and unload the dll, so a new build can be loaded without restarting the game{{/if}}.

## Your first hook

Open `src\hooks.cpp`. The steps are at the top of the file: find the function in a disassembler, put the
bytes at its start into `kExamplePattern` with `??` for every byte that is part of an address, and make
`ExampleFn` its real signature. Build, start the game, and the log says `example hook: placed at ...`, then
`first call` the first time the game runs the function.

To add a hook, copy the example block (pattern, type, original, detour, the function that places it) under a
new name and call it from `hooks::Install`.

## When it does not load

- **No log file and no console:** the dll was never loaded.
{{#if Mode == proxy}}
  - It is not next to the exe that runs the game. Many games start a small launcher exe in the top folder
    and the real exe lives deeper (Unreal: `<Project>\Binaries\Win64`; some Unity launchers too).
  - The bitness is wrong: a {{#if Arch == x64}}64-bit{{#else}}32-bit{{/if}} dll in a {{#if Arch == x64}}32-bit{{#else}}64-bit{{/if}} game. The game then usually fails to
    start with error 0xc000007b.
  - The game never asks for `version.dll`. Rare, but it happens; then build the plain dll and inject it, or
    proxy another dll the game imports.
  - Another mod already uses `version.dll` in that folder (some mod loaders do). Two cannot share the name.
{{#elif Mode == asi}}
  - The game has no ASI loader, or the loader looks in another folder (`scripts`, `plugins`).
  - The bitness is wrong: this is a {{#if Arch == x64}}64-bit{{#else}}32-bit{{/if}} plugin.
{{#else}}
  - The injector reported success but the bitness is wrong: this is a {{#if Arch == x64}}64-bit{{#else}}32-bit{{/if}} dll.
  - The game has an anti-cheat that blocks injection. This project does not get around that.
{{/if}}
{{#if GameExe}}
- **The dll loads but does nothing:** it only runs inside `{{GameExe}}.exe` (see `IsTargetProcess` in
  `src\dllmain.cpp`). If the game's real exe has another name, change it there.
{{/if}}
- **"pattern not found":** the bytes are not in the exe. Either the pattern contains bytes that change
  between builds (wildcard them), the function lives in a dll and not in the exe (pass that module to
  `FindPattern`), or the game unpacks its code after start (raise `kStartDelayMs` in `src\dllmain.cpp`).
- **The game crashes when the hooked function runs:** the detour's signature does not match the function
  (argument count, types, return type{{#if Arch == x86}}, calling convention{{/if}}), or the pattern matched a different function.
- **The game crashes or freezes when the dll loads:** something slow or complex was added to `DllMain`. Keep
  it as it is and do the work in `MainThread`.
{{#if Console}}
- **Closing the console window closes the game:** that is how Windows consoles work. Turn the console off by
  passing `false` to `modlog::Open` in `src\dllmain.cpp`; the log file stays.
{{/if}}
- **Antivirus removes the dll:** dlls that hook other programs look suspicious to scanners. Add an exclusion
  for the build folder and the game folder.
{{#if Mode == proxy}}
- **Online games:** an anti-cheat can treat a proxy dll as tampering. Use it in single-player or offline
  games.
{{/if}}
