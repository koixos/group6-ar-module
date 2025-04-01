#!/bin/bash

source ./setup_android_build.sh

mkdir -p build/android/arm64-v8a
mkdir -p build/android/armeabi-v7a
mkdir -p build/android/x86
mkdir -p build/android/x86_64

cd build/android/arm64-v8a
cmake -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_ROOT/build/cmake/android.toolchain.cmake \
      -DANDROID_ABI=arm64-v8a \
      -DANDROID_PLATFORM=android-24 \
      -DCMAKE_BUILD_TYPE=Release \
      ../../../

make -j4

cd ../armeabi-v7a
cmake -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_ROOT/build/cmake/android.toolchain.cmake \
      -DANDROID_ABI=armeabi-v7a \
      -DANDROID_PLATFORM=android-24 \
      -DCMAKE_BUILD_TYPE=Release \
      ../../../

make -j4

cd ../x86
cmake -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_ROOT/build/cmake/android.toolchain.cmake \
      -DANDROID_ABI=x86 \
      -DANDROID_PLATFORM=android-24 \
      -DCMAKE_BUILD_TYPE=Release \
      ../../../

make -j4

cd ../x86_64
cmake -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_ROOT/build/cmake/android.toolchain.cmake \
      -DANDROID_ABI=x86_64 \
      -DANDROID_PLATFORM=android-24 \
      -DCMAKE_BUILD_TYPE=Release \
      ../../../

make -j4

echo "Android builds completed"

mkdir -p ../../../Unity/Assets/Plugins/Android/libs/arm64-v8a
mkdir -p ../../../Unity/Assets/Plugins/Android/libs/armeabi-v7a
mkdir -p ../../../Unity/Assets/Plugins/Android/libs/x86
mkdir -p ../../../Unity/Assets/Plugins/Android/libs/x86_64

cp build/android/arm64-v8a/libnetworking.so ../../../Unity/Assets/Plugins/Android/libs/arm64-v8a/
cp build/android/armeabi-v7a/libnetworking.so ../../../Unity/Assets/Plugins/Android/libs/armeabi-v7a/
cp build/android/x86/libnetworking.so ../../../Unity/Assets/Plugins/Android/libs/x86/
cp build/android/x86_64/libnetworking.so ../../../Unity/Assets/Plugins/Android/libs/x86_64/