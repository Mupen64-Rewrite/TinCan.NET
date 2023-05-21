add_library(tincan-bridge MODULE
  "plugin.cpp"
)
target_compile_features(tincan-bridge PUBLIC cxx_std_20)
target_link_libraries(tincan-bridge PUBLIC fmt::fmt PkgConfig::zeromq PkgConfig::msgpack m64p::api)