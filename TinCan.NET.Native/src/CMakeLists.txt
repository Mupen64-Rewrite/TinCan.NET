macro(tc_link_dependencies target)
  target_compile_features(${target} PUBLIC cxx_std_20)
  if(DEFINED VCPKG_VERBOSE)
    target_link_libraries(${target} PUBLIC fmt::fmt cppzmq msgpack-cxx m64p::api)
  else()
    target_link_libraries(${target} PUBLIC fmt::fmt cppzmq msgpackc-cxx m64p::api)
  endif()
  if(MSVC)
    set_target_properties(${target} PROPERTIES
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
    )
    target_link_libraries(${target} PRIVATE bcrypt.lib)
  endif()
endmacro()

add_library(tincan-bridge MODULE
  "duthomhas/csprng.cpp"
  "duthomhas/csprng.h"
  "duthomhas/csprng.hpp"
  "duthomhas/is_iterable.hpp"
  "ipc/postbox.hpp"
  "ipc/safe_queue.hpp"
  "tl/function_ref.hpp"
  "plugin.cpp"
  "global.cpp"
  "global.hpp"
)
tc_link_dependencies(tincan-bridge)
