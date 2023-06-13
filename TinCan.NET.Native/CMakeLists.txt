cmake_minimum_required(VERSION 3.24)
project(TinCan.NET.Native
  VERSION 0.1.0
  DESCRIPTION "Native bridge plugin for TinCan.NET"
  LANGUAGES CXX
)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_LIST_DIR}/cmake")

if(DEFINED VCPKG_VERBOSE)
  find_package(fmt REQUIRED)
  find_package(cppzmq REQUIRED)
  find_package(msgpack-cxx REQUIRED)
  # set dependency variables
  set(tc_deps.fmt fmt::fmt)
  set(tc_deps.cppzmq cppzmq)
  set(tc_deps.msgpack msgpack-cxx)
else()
  find_package(fmt REQUIRED)
  find_package(Boost REQUIRED)
  find_package(PkgConfig REQUIRED)
  pkg_check_modules(zeromq REQUIRED IMPORTED_TARGET
    "libzmq >= 4.3.4"
  )
  pkg_check_modules(msgpack REQUIRED IMPORTED_TARGET
    "msgpack"
  )
  # set dependency variables
  set(tc_deps.fmt fmt::fmt)
  set(tc_deps.cppzmq PkgConfig::zeromq)
  set(tc_deps.msgpack PkgConfig::msgpack)
endif()
include(M64PApi)

# add_executable(tester "test.cpp")
# target_link_libraries(tester PUBLIC PkgConfig::zeromq PkgConfig::msgpack)

add_subdirectory(src)