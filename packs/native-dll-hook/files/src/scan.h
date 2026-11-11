// Finds code in a loaded module by its bytes, so a hook survives game updates that move the function.
#pragma once

#include <windows.h>
#include <cstdint>

namespace scan
{
    // Searches the executable sections of a module for a byte pattern and returns the address of the first
    // match, or 0. The pattern is hex bytes separated by spaces; ?? (or ?) matches any byte:
    //     "48 89 5C 24 ?? 57 48 83 EC 20"
    // Pass GetModuleHandleW(nullptr) for the game's exe, or GetModuleHandleW(L"Some.dll") for a dll.
    uintptr_t FindPattern(HMODULE module, const char* pattern);

    // The address a relative call or jump points to. instruction is the address of the E8/E9 byte.
    // Use it when the pattern matches a call to the function instead of the function itself.
    uintptr_t FollowRelative(uintptr_t instruction);
}
