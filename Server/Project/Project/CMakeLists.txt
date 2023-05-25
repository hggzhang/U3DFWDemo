cmake_minimum_required(VERSION 3.0)

project(Server)

include_directories(.)
include_directories(Net)
include_directories(Net/Socket)
include_directories(3rd/lua-5.3.5/src)

file(GLOB_RECURSE SOURCES "*.cpp" "*.h" "Net/*.h" "Net/*.cpp" "Net/Socket/*.h" "Net/Socket/*.cpp" "3rd/lua-5.3.5/src/*.h" "3rd/lua-5.3.5/src/*.c")

add_executable(Run ${SOURCES})

target_link_libraries(Run ws2_32)