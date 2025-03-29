cmake_minimum_required(VERSION 3.15)
project(group6)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_TOOLCHAIN_FILE "C:/Tools/vcpkg/scripts/buildsystems/vcpkg.cmake")

include_directories("C:/Tools/vcpkg/installed/x64-mingw-dynamic/include")

find_package(unofficial-uwebsockets CONFIG REQUIRED)

add_executable(Server server.cpp)

target_link_libraries(Server PRIVATE
    unofficial::uwebsockets::uwebsockets
    kernel32
    user32
    gdi32
    winspool
    shell32
    ole32
    oleaut32
    uuid
    comdlg32
    advapi32
)

install(
    FILES
    ${VCPKG_INSTALLED_DIR}/x64-mingw-dynamic/bin/uwebsockets.dll
    DESTINATION
    ${CMAKE_CURRENT_BINARY_DIR}
)