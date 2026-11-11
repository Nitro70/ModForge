// A log file next to the dll, mirrored to a console window when one is open.
#pragma once

#include <windows.h>

namespace modlog
{
    // Opens <dll folder>\<name>.log (replacing the old one) and, when asked, a console window.
    void Open(HMODULE self, const wchar_t* name, bool console);

    // printf style. Adds the time and a line break. Safe to call from any thread.
    void Write(const char* format, ...);

    // Closes the file and the console this dll opened.
    void Close();
}
