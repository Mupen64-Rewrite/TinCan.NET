macro(tc_link_dependencies target)
  target_compile_features(${target} PUBLIC cxx_std_20)
  target_link_libraries(${target} PUBLIC ${tc_deps.fmt} ${tc_deps.cppzmq} ${tc_deps.msgpack} m64p::api)
  if(MSVC)
    set_target_properties(${target} PROPERTIES
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
    )
    target_link_libraries(${target} PRIVATE bcrypt.lib)
  endif()
endmacro()

add_executable(tincan-client
  "test_client.cpp"
)
tc_link_dependencies(tincan-client)

# add_library(tincan-bridge MODULE
#   "plugin.cpp"
# )
# tc_link_dependencies(tincan-bridge)
