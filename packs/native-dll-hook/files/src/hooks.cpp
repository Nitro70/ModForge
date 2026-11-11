#include "hooks.h"

#include "log.h"
#include "scan.h"

#include <windows.h>
#include <MinHook.h>

// ---------------------------------------------------------------------------------------------------------
// Example hook.
//
// 1. Find the function in a disassembler or debugger (x64dbg, Ghidra, IDA, Cheat Engine).
// 2. Copy the bytes at its start into kExamplePattern, as hex with spaces. Replace every byte that belongs
//    to an address or an offset with ??, because those change with each game update. Make it long enough to
//    match only this one function.
// 3. Make ExampleFn the function's real signature: its return type, its parameters, its calling convention.
//    A wrong signature crashes the game when the hook runs.
//
// While the pattern is empty nothing is hooked and the log says so.
// ---------------------------------------------------------------------------------------------------------

static const char* const kExamplePattern = "";

{{#if Arch == x64}}
// On 64-bit Windows there is one calling convention: the first four arguments are in rcx, rdx, r8 and r9.
// For a C++ member function the first argument is the object (this).
using ExampleFn = void* (*)(void* a1, void* a2, void* a3, void* a4);

// The original function. Filled in by MinHook; call it to run the game's own code.
static ExampleFn g_exampleOriginal = nullptr;

static void* ExampleDetour(void* a1, void* a2, void* a3, void* a4)
{
    // The game calls this in place of the original, on whatever thread the game uses. Keep it short.
    static bool logged = false;
    if (!logged)
    {
        logged = true;
        modlog::Write("example hook: first call, a1=%p", a1);
    }

    // TODO: your code here. Change the arguments, skip the call, or change what it returns.
    return g_exampleOriginal(a1, a2, a3, a4);
}
{{#else}}
// On 32-bit Windows the calling convention matters: __cdecl, __stdcall, __fastcall or __thiscall. A C++
// member function is __thiscall (the object in ecx); hook it with a __fastcall detour whose first two
// parameters are the object and an unused value for edx, as below.
using ExampleFn = void* (__thiscall*)(void* self, void* a1);

// The original function. Filled in by MinHook; call it to run the game's own code.
static ExampleFn g_exampleOriginal = nullptr;

static void* __fastcall ExampleDetour(void* self, void* /*edx*/, void* a1)
{
    // The game calls this in place of the original, on whatever thread the game uses. Keep it short.
    static bool logged = false;
    if (!logged)
    {
        logged = true;
        modlog::Write("example hook: first call, self=%p", self);
    }

    // TODO: your code here. Change the arguments, skip the call, or change what it returns.
    return g_exampleOriginal(self, a1);
}
{{/if}}

static void HookExample()
{
    if (kExamplePattern[0] == '\0')
    {
        modlog::Write("example hook: no pattern yet. Put one into kExamplePattern in src/hooks.cpp.");
        return;
    }

    // The game's exe. For a function in a dll use GetModuleHandleW(L"Name.dll").
    const uintptr_t target = scan::FindPattern(GetModuleHandleW(nullptr), kExamplePattern);
    if (!target)
    {
        modlog::Write("example hook: pattern not found. The game was updated, or the code is not unpacked yet.");
        return;
    }

    const MH_STATUS created = MH_CreateHook(reinterpret_cast<LPVOID>(target), reinterpret_cast<LPVOID>(&ExampleDetour),
                                            reinterpret_cast<LPVOID*>(&g_exampleOriginal));
    if (created != MH_OK)
    {
        modlog::Write("example hook: MH_CreateHook failed: %s", MH_StatusToString(created));
        return;
    }
    modlog::Write("example hook: placed at %p", reinterpret_cast<void*>(target));
}

bool hooks::Install()
{
    const MH_STATUS started = MH_Initialize();
    if (started != MH_OK)
    {
        modlog::Write("MH_Initialize failed: %s", MH_StatusToString(started));
        return false;
    }

    // Create every hook first, then switch them all on together.
    HookExample();

    const MH_STATUS enabled = MH_EnableHook(MH_ALL_HOOKS);
    if (enabled != MH_OK)
        modlog::Write("MH_EnableHook failed: %s", MH_StatusToString(enabled));
    return true;
}

void hooks::Remove()
{
    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();
}
