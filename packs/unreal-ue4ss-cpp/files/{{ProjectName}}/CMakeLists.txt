{{#if Ue4ss == stable}}
cmake_minimum_required(VERSION 3.18)

set(TARGET {{ProjectName}})
project(${TARGET})

add_library(${TARGET} SHARED "dllmain.cpp")
target_include_directories(${TARGET} PRIVATE .)
target_link_libraries(${TARGET} PUBLIC UE4SS)
{{#else}}
set(TARGET {{ProjectName}})

add_library(${TARGET} SHARED
    dllmain.cpp
)

target_include_directories(${TARGET} PRIVATE .)
target_link_libraries(${TARGET} PUBLIC UE4SS)
{{/if}}
