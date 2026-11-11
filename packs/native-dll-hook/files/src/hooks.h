// The mod's hooks: what the dll changes in the game.
#pragma once

namespace hooks
{
    // Starts MinHook, finds the functions and places the hooks. Returns false when MinHook itself fails.
    bool Install();

    // Takes every hook out again and shuts MinHook down.
    void Remove();
}
