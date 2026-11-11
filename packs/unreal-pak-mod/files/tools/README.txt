pack.bat looks in this folder for the packing tools. Put one of these here (or have it on PATH):

  UnrealPak.exe   from Engine\Binaries\Win64 of an Unreal Engine install. Use the game's engine version ({{EngineVersion}}).
                  If a copy of the exe does not start here, leave it in the engine and set the UNREALPAK
                  environment variable to its full path instead.
  repak.exe       https://github.com/trumank/repak/releases   (one file, no engine install needed)
{{#if Format == iostore}}

and, for the IoStore conversion:

  retoc.exe       https://github.com/trumank/retoc/releases
{{/if}}
