// {{ModName}} {{Version}} by {{Author}}
//
// Entry point of the dll. Windows calls DllMain while it is still loading the dll, and almost nothing is
// safe to do there, so DllMain only starts a thread. The thread does the real work: open the log, place the
// hooks, then wait for the unload key.

#include <windows.h>

#include "hooks.h"
#include "log.h"

namespace
{
    const wchar_t* const kModName = L"{{ProjectName}}";

    // Press this key while the game window is in front to take the hooks out again.
    const int kUnloadKey = VK_{{UnloadKey}};

    // Wait this long before placing hooks. Raise it when the game unpacks or decrypts its code at start
    // (some copy protections do) and the pattern is only there a moment later.
    const DWORD kStartDelayMs = 0;

    HMODULE g_self = nullptr;

    // The file name of the exe this dll was loaded into.
    const wchar_t* HostExeName(wchar_t* buffer, DWORD size)
    {
        GetModuleFileNameW(nullptr, buffer, size);
        const wchar_t* slash = wcsrchr(buffer, L'\\');
        return slash ? slash + 1 : buffer;
    }

    bool IsTargetProcess()
    {
{{#if GameExe}}
        // Other programs in the game's folder (launchers, crash reporters) load the dll too. Stay idle there.
        wchar_t path[MAX_PATH] = {};
        return _wcsicmp(HostExeName(path, MAX_PATH), L"{{GameExe|str}}.exe") == 0;
{{#else}}
        // To run only in the game and not in its launcher or crash reporter, compare the exe name here:
        //     wchar_t path[MAX_PATH] = {};
        //     return _wcsicmp(HostExeName(path, MAX_PATH), L"Game.exe") == 0;
        return true;
{{/if}}
    }

    bool GameWindowIsInFront()
    {
        DWORD process = 0;
        GetWindowThreadProcessId(GetForegroundWindow(), &process);
        return process == GetCurrentProcessId();
    }

    DWORD WINAPI MainThread(LPVOID)
    {
{{#if Console}}
        modlog::Open(g_self, kModName, true);
{{#else}}
        modlog::Open(g_self, kModName, false);
{{/if}}

        wchar_t path[MAX_PATH] = {};
        modlog::Write("%ls {{Version}} loaded into %ls", kModName, HostExeName(path, MAX_PATH));

        if (kStartDelayMs > 0)
            Sleep(kStartDelayMs);

        if (hooks::Install())
            modlog::Write("ready. Press {{UnloadKey}} in the game to remove the hooks.");

        while (!(GameWindowIsInFront() && (GetAsyncKeyState(kUnloadKey) & 0x8000)))
            Sleep(50);

        hooks::Remove();
        // Give a thread that is inside a hook right now time to leave it.
        Sleep(200);
{{#if Mode == proxy}}
        modlog::Write("hooks removed. The dll stays loaded: the game holds on to version.dll.");
        modlog::Close();
        return 0;
{{#else}}
        modlog::Write("hooks removed, unloading");
        modlog::Close();
        FreeLibraryAndExitThread(g_self, 0);
{{/if}}
    }
}

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        g_self = module;
        DisableThreadLibraryCalls(module);
        if (IsTargetProcess())
        {
            const HANDLE thread = CreateThread(nullptr, 0, MainThread, nullptr, 0, nullptr);
            if (thread)
                CloseHandle(thread);
        }
    }
    return TRUE;
}
