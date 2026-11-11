#include "scan.h"

#include <cstdlib>
#include <cstring>
#include <vector>

namespace
{
    // "48 8B ?? 05" becomes { 0x48, 0x8B, -1, 0x05 }. Returns an empty list when the text is not a pattern.
    std::vector<int> Parse(const char* pattern)
    {
        std::vector<int> bytes;
        const char* p = pattern;
        while (*p)
        {
            if (*p == ' ')
            {
                ++p;
                continue;
            }
            if (*p == '?')
            {
                bytes.push_back(-1);
                while (*p == '?')
                    ++p;
                continue;
            }
            char* end = nullptr;
            const unsigned long value = strtoul(p, &end, 16);
            if (end == p || value > 0xFF)
                return {};
            bytes.push_back(static_cast<int>(value));
            p = end;
        }
        return bytes;
    }

    uintptr_t FindInRange(const uint8_t* begin, const uint8_t* end, const std::vector<int>& bytes)
    {
        const size_t count = bytes.size();
        if (static_cast<size_t>(end - begin) < count)
            return 0;
        for (const uint8_t* at = begin; at <= end - count; ++at)
        {
            size_t i = 0;
            while (i < count && (bytes[i] < 0 || at[i] == bytes[i]))
                ++i;
            if (i == count)
                return reinterpret_cast<uintptr_t>(at);
        }
        return 0;
    }
}

uintptr_t scan::FindPattern(HMODULE module, const char* pattern)
{
    const std::vector<int> bytes = Parse(pattern);
    if (!module || bytes.empty())
        return 0;

    const auto base = reinterpret_cast<const uint8_t*>(module);
    const auto dos = reinterpret_cast<const IMAGE_DOS_HEADER*>(base);
    if (dos->e_magic != IMAGE_DOS_SIGNATURE)
        return 0;
    const auto nt = reinterpret_cast<const IMAGE_NT_HEADERS*>(base + dos->e_lfanew);
    if (nt->Signature != IMAGE_NT_SIGNATURE)
        return 0;

    const IMAGE_SECTION_HEADER* section = IMAGE_FIRST_SECTION(nt);
    for (unsigned i = 0; i < nt->FileHeader.NumberOfSections; ++i, ++section)
    {
        if (!(section->Characteristics & IMAGE_SCN_MEM_EXECUTE))
            continue;

        // Walk the section region by region and skip memory that cannot be read.
        const uint8_t* at = base + section->VirtualAddress;
        const uint8_t* const sectionEnd = at + section->Misc.VirtualSize;
        while (at < sectionEnd)
        {
            MEMORY_BASIC_INFORMATION info;
            if (!VirtualQuery(at, &info, sizeof(info)))
                break;
            const uint8_t* regionEnd = static_cast<const uint8_t*>(info.BaseAddress) + info.RegionSize;
            if (regionEnd > sectionEnd)
                regionEnd = sectionEnd;
            const bool readable = info.State == MEM_COMMIT && !(info.Protect & (PAGE_NOACCESS | PAGE_GUARD));
            if (readable)
            {
                if (const uintptr_t found = FindInRange(at, regionEnd, bytes))
                    return found;
            }
            at = regionEnd;
        }
    }
    return 0;
}

uintptr_t scan::FollowRelative(uintptr_t instruction)
{
    int32_t offset = 0;
    memcpy(&offset, reinterpret_cast<const void*>(instruction + 1), sizeof(offset));
    return instruction + 5 + offset;
}
