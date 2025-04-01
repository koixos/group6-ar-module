cmake_minimum_required(VERSION 3.12)
project(NetworkPlugin)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_TOOLCHAIN_FILE "C:/Tools/vcpkg/scripts/buildsystems/vcpkg.cmake")

include_directories("C:/Tools/vcpkg/installed/x64-windows/include")

find_package(unofficial-uwebsockets CONFIG REQUIRED)
find_package(nlohmann_json CONFIG REQUIRED)

set(SOURCES
    src/NetworkInterface.cpp
)

set(HEADERS
    src/NetworkInterface.h
)

add_library(networking SHARED ${SOURCES} ${HEADERS})

target_include_directories(networking PRIVATE
    ${UWEBSOCKETS_INCLUDE_DIR}
    ${NLOHMANN_JSON_INCLUDE_DIRS}
)

target_link_libraries(networking PRIVATE
    unofficial::uwebsockets::uwebsockets
    nlohmann_json::nlohmann_json
)

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

install(TARGETS networking
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
    RUNTIME DESTINATION bin
)