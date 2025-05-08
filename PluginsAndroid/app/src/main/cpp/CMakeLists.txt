cmake_minimum_required(VERSION 3.10)

set(ANDROID_ABI "arm64-v8a")
set(ANDROID_PLATFORM android-21)
set(ANDROID_NDK "C:/Users/koios/AppData/Local/Android/Sdk/ndk/27.0.12077973")
set(CMAKE_TOOLCHAIN_FILE ${ANDROID_NDK}/build/cmake/android.toolchain.cmake)

project("UnityWebSocketPlugin")

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_ANDROID_ARCH_ABI arm64-v8a)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/Unity/Assets/Plugins/Android/${ANDROID_ABI})

include_directories(
        "${CMAKE_SYSROOT}/usr/include"
        "${CMAKE_SYSROOT}/usr/include/android"
)

add_definitions(-DASIO_STANDALONE)

add_library(UnityWebSocketPlugin SHARED
        WebSocketPlugin.cpp
)

target_include_directories(UnityWebSocketPlugin PRIVATE
        ${CMAKE_SOURCE_DIR}/include
        ${CMAKE_ANDROID_NDK}/sysroot/usr/include
)

target_link_libraries(UnityWebSocketPlugin
        log
        android
        atomic
        m
)
