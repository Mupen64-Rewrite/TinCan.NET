add_library(tincan-bridge MODULE
  "plugin.cpp"
)
target_compile_features(tincan-bridge PUBLIC cxx_std_20)
target_link_libraries(tincan-bridge PUBLIC ${tc_deps.fmt} ${tc_deps.cppzmq} ${tc_deps.msgpack} m64p::api)
if(MSVC)
  set_target_properties(tincan-bridge PROPERTIES
    MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )
  target_link_libraries(tincan-bridge PRIVATE bcrypt.lib)
endif()