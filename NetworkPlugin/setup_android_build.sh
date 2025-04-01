#!/bin/bash

export ANDROID_NDK_ROOT="C:/User/Zeynep/AppData/Local/Android/Sdk/ndk/29.0.13113456"
export PATH=$ANDROID_NDK_ROOT/toolchains/llvm/prebuilt/windows-x86_64/bin:$PATH
export CC=$ANDROID_NDK_ROOT/toolchains/llvm/prebuilt/windows-x86_64/bin/clang
export CXX=$ANDROID_NDK_ROOT/toolchains/llvm/prebuilt/windows-x86_64/bin/clang++
