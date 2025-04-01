- to compile CMakeLists.txt:
* mkdir build
* cd build
* cmake -DCMAKE_TOOLCHAIN_FILE=C:/User/Zeynep/AppData/Local/Android/Sdk/ndk/29.0.13113456/build/cmake/android.toolchain.cmake -DANDROID_ABI=arm64-v8a -DANDROID_PLATFORM=android-24 -DCMAKE_BUILD_TYPE=Release ..
* bash setup_android_build.sh
* bash build_android.sh