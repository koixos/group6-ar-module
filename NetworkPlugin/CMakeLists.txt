cmake_minimum_required(VERSION 3.10)
project(UnityWebSocketPlugin)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
#set(CMAKE_TOOLCHAIN_FILE "C:/Tools/vcpkg/scripts/buildsystems/vcpkg.cmake")

if(NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE Release)
endif()

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

if(WIN32)
    set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)
endif()

#include_directories("C:/Tools/vcpkg/installed/x64-windows/include")

find_package(unofficial-uwebsockets CONFIG REQUIRED)
#find_package(nlohmann_json CONFIG REQUIRED)

set(SOURCES
    src/WebSocketPlugin.cpp
)

set(HEADERS
    src/WebSocketPlugin.h
)

add_library(UnityWebSocketPlugin SHARED ${SOURCES} ${HEADERS})

#[===[
target_include_directories(networking PRIVATE
#    ${UWEBSOCKETS_INCLUDE_DIR}
#    ${NLOHMANN_JSON_INCLUDE_DIRS}
)
]===]

target_link_libraries(UnityWebSocketPlugin PRIVATE
    unofficial::uwebsockets::uwebsockets
#    nlohmann_json::nlohmann_json
)

if(MSVC)
    target_compile_options(UnityWebSocketPlugin PRIVATE /W4)
else()
    target_compile_options(UnityWebSocketPlugin PRIVATE -Wall -Wextra)
endif()

#[===[
if(ANDROID)
    set_target_properties(networking PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/output/android/${ANDROID_ABI}"
    )
elseif(IOS)
    set_target_properties(networking PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/output/ios"
    )
elseif(WIN32)
    set_target_properties(networking PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/output/win/x86_64"
    )
elseif(APPLE)
    set_target_properties(networking PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/output/mac"
    )
else()
    set_target_properties(networking PROPERTIES
        LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/output/linux/x86_64"
    )
endif()
]===]

install(TARGETS UnityWebSocketPlugin
    LIBRARY DESTINATION ${CMAKE_INSTALL_PREFIX}/lib
    RUNTIME DESTINATION ${CMAKE_INSTALL_PREFIX}/bin
)