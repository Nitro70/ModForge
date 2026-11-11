{{#if Ue4ss == stable}}
cmake_minimum_required(VERSION 3.18)

{{#else}}
cmake_minimum_required(VERSION 3.22)
{{/if}}
project({{ProjectName}}Workspace)

# RE-UE4SS is a git submodule (setup.bat adds it). It must come first: the mod links against its UE4SS target.
add_subdirectory(RE-UE4SS)
add_subdirectory({{ProjectName}})
