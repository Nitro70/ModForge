cmake_minimum_required(VERSION 3.20)
project({{ProjectName}} LANGUAGES C CXX)

if(NOT MSVC)
    message(FATAL_ERROR "Build this with Visual Studio (MSVC or clang-cl). MinGW is not supported.")
endif()
{{#if Arch == x64}}
if(NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "This mod is for a 64-bit game. Configure with: cmake -S . -B build -A x64")
endif()
{{#else}}
if(NOT CMAKE_SIZEOF_VOID_P EQUAL 4)
    message(FATAL_ERROR "This mod is for a 32-bit game. Configure with: cmake -S . -B build -A Win32")
endif()
{{/if}}

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Static C runtime: the dll then needs no Visual C++ redistributable on the player's PC.
# The policy line makes the setting reach MinHook's own CMake project too.
set(CMAKE_POLICY_DEFAULT_CMP0091 NEW)
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

# MinHook is downloaded at configure time (needs git).
include(FetchContent)
FetchContent_Declare(minhook
    GIT_REPOSITORY https://github.com/TsudaKageyu/minhook.git
    GIT_TAG {{MinHookTag}}
    GIT_SHALLOW TRUE
)
FetchContent_MakeAvailable(minhook)

add_library({{ProjectName}} SHARED
    src/dllmain.cpp
    src/hooks.cpp
    src/log.cpp
    src/scan.cpp
{{#if Mode == proxy}}
    src/proxy_exports.cpp
{{/if}}
)
target_include_directories({{ProjectName}} PRIVATE src)
target_link_libraries({{ProjectName}} PRIVATE minhook)
target_compile_definitions({{ProjectName}} PRIVATE WIN32_LEAN_AND_MEAN NOMINMAX UNICODE _UNICODE _CRT_SECURE_NO_WARNINGS)
{{#if Mode == proxy}}

# The file must be called version.dll: Windows loads it in place of the system one because it sits next to
# the game's exe. src/proxy_exports.cpp forwards every export to the real file.
set_target_properties({{ProjectName}} PROPERTIES OUTPUT_NAME "version")
{{#elif Mode == asi}}

# An ASI plugin is a dll with the extension .asi.
set_target_properties({{ProjectName}} PROPERTIES SUFFIX ".asi")
{{/if}}
{{#if Deploy && GamePath && Mode != inject}}

# Copy the result next to the game's exe after every build. Fails while the game is running (the file is in use).
{{#if UnrealBinDir}}
set(GAME_DIR "{{GamePath|fwd}}/{{UnrealBinDir|fwd}}")
{{#else}}
set(GAME_DIR "{{GamePath|fwd}}")
{{/if}}
add_custom_command(TARGET {{ProjectName}} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "$<TARGET_FILE:{{ProjectName}}>" "${GAME_DIR}/$<TARGET_FILE_NAME:{{ProjectName}}>"
    COMMENT "Copying $<TARGET_FILE_NAME:{{ProjectName}}> to ${GAME_DIR}"
)
{{/if}}
