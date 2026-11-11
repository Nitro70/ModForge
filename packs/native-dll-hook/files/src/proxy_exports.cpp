// Proxy exports for version.dll.
//
// This dll is named version.dll and sits next to the game's exe, so Windows loads it in place of the system
// file. Everything that then asks for a version.dll function must still get the real one. Each line below
// tells the linker to export a name that forwards to the same function in the real file. The path goes
// through \\.\GLOBALROOT\SystemRoot so it points at the Windows folder whatever drive it is on, and cannot
// resolve back to this file.
//
// The list is the complete export table of version.dll (17 functions, the same on Windows 10 and 11).
// This file contains no code. Do not call any version.dll function from the mod: the dll would import itself.

{{#if Arch == x64}}
#define FORWARD(name, ordinal) \
    __pragma(comment(linker, "/EXPORT:" name "=\\\\.\\GLOBALROOT\\SystemRoot\\System32\\version.dll." name ",@" #ordinal))
{{#else}}
// A 32-bit game on 64-bit Windows gets its system dlls from SysWOW64.
#define FORWARD(name, ordinal) \
    __pragma(comment(linker, "/EXPORT:" name "=\\\\.\\GLOBALROOT\\SystemRoot\\SysWOW64\\version.dll." name ",@" #ordinal))
{{/if}}

FORWARD("GetFileVersionInfoA", 1)
FORWARD("GetFileVersionInfoByHandle", 2)
FORWARD("GetFileVersionInfoExA", 3)
FORWARD("GetFileVersionInfoExW", 4)
FORWARD("GetFileVersionInfoSizeA", 5)
FORWARD("GetFileVersionInfoSizeExA", 6)
FORWARD("GetFileVersionInfoSizeExW", 7)
FORWARD("GetFileVersionInfoSizeW", 8)
FORWARD("GetFileVersionInfoW", 9)
FORWARD("VerFindFileA", 10)
FORWARD("VerFindFileW", 11)
FORWARD("VerInstallFileA", 12)
FORWARD("VerInstallFileW", 13)
FORWARD("VerLanguageNameA", 14)
FORWARD("VerLanguageNameW", 15)
FORWARD("VerQueryValueA", 16)
FORWARD("VerQueryValueW", 17)
