cmake_minimum_required(VERSION 3.21)
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
if ("${CMAKE_SYSTEM_NAME}" STREQUAL "iOS" OR IOS)
    set(CMAKE_OSX_ARCHITECTURES "arm64")
else()
    set(CMAKE_OSX_ARCHITECTURES "arm64;x86_64")
endif()
set(CMAKE_CXX_VISIBILITY_PRESET hidden)

project({{ProjectName}} VERSION {{Version}})

# Every .cpp file under src, however deep.
file(GLOB_RECURSE SOURCES CONFIGURE_DEPENDS src/*.cpp)

# The mod is a shared library.
add_library(${PROJECT_NAME} SHARED ${SOURCES})

# The Geode SDK is found through the GEODE_SDK environment variable,
# which "geode sdk install" sets.
if (NOT DEFINED ENV{GEODE_SDK})
    message(FATAL_ERROR "Unable to find Geode SDK! Please define GEODE_SDK environment variable to point to Geode")
else()
    message(STATUS "Found Geode: $ENV{GEODE_SDK}")
endif()

add_subdirectory($ENV{GEODE_SDK} ${CMAKE_CURRENT_BINARY_DIR}/geode)

# Reads mod.json, links Geode, and packs the result into <mod id>.geode.
setup_geode_mod(${PROJECT_NAME})
