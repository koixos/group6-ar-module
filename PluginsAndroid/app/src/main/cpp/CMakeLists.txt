cmake_minimum_required(VERSION 3.10)
project("UnityWebSocketPlugin")

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include_directories(
        "${CMAKE_SYSROOT}/usr/include"
        "${CMAKE_SYSROOT}/usr/include/android"
)

add_definitions(-DASIO_STANDALONE)

add_library(
        UnityWebSocketPlugin SHARED
        WebSocketPlugin.cpp
)

target_include_directories(
        UnityWebSocketPlugin PRIVATE
        ${CMAKE_SOURCE_DIR}/include
        ${CMAKE_ANDROID_NDK}/sysroot/usr/include
)

target_link_libraries(
        UnityWebSocketPlugin
        log
        -pthread
)