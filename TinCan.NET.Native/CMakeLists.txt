cmake_minimum_required(VERSION 3.24)
project(TinCan.NET.Native
  VERSION 0.1.0
  DESCRIPTION "Native bridge plugin for TinCan.NET"
  LANGUAGES CXX ASM
)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_LIST_DIR}/cmake")

find_package(fmt REQUIRED)
find_package(PkgConfig REQUIRED)
pkg_check_modules(zeromq REQUIRED IMPORTED_TARGET
  "libzmq >= 4.3.4"
)
pkg_check_modules(msgpack REQUIRED IMPORTED_TARGET
  "msgpack"
)
include(M64PApi)

message(STATUS "Using assembler: ${CMAKE_ASM_COMPILER_ID}")

# add_executable(tester "test.cpp")
# target_link_libraries(tester PUBLIC PkgConfig::zeromq PkgConfig::msgpack)

add_subdirectory(src)