#include "log.h"

#include <cstdarg>
#include <cstdio>
#include <mutex>
#include <string>

namespace
{
    std::mutex g_mutex;
    FILE* g_file = nullptr;
    FILE* g_console = nullptr;
    bool g_ownsConsole = false;
}

void modlog::Open(HMODULE self, const wchar_t* name, bool console)
{
    std::lock_guard<std::mutex> lock(g_mutex);

    // The log goes next to the dll, wherever that is.
    wchar_t path[MAX_PATH] = {};
    GetModuleFileNameW(self, path, MAX_PATH);
    std::wstring file(path);
    const size_t slash = file.find_last_of(L"\\/");
    file = (slash == std::wstring::npos ? std::wstring() : file.substr(0, slash + 1)) + name + L".log";
    g_file = _wfopen(file.c_str(), L"w");

    if (console)
    {
        // AllocConsole fails when the game already has a console; then we just write to that one.
        g_ownsConsole = AllocConsole() != FALSE;
        g_console = _wfopen(L"CONOUT$", L"w");
        if (g_ownsConsole)
            SetConsoleTitleW(name);
    }
}

void modlog::Write(const char* format, ...)
{
    char message[1024];
    va_list args;
    va_start(args, format);
    vsnprintf(message, sizeof(message), format, args);
    va_end(args);

    SYSTEMTIME now;
    GetLocalTime(&now);

    std::lock_guard<std::mutex> lock(g_mutex);
    for (FILE* out : { g_file, g_console })
    {
        if (!out)
            continue;
        fprintf(out, "[%02d:%02d:%02d.%03d] %s\n", now.wHour, now.wMinute, now.wSecond, now.wMilliseconds, message);
        fflush(out);
    }
}

void modlog::Close()
{
    std::lock_guard<std::mutex> lock(g_mutex);
    if (g_file)
        fclose(g_file);
    if (g_console)
        fclose(g_console);
    g_file = nullptr;
    g_console = nullptr;
    if (g_ownsConsole)
        FreeConsole();
    g_ownsConsole = false;
}
