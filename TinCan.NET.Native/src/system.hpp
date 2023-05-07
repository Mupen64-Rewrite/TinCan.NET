#ifndef TINCAN_SYSTEM_HPP
#define TINCAN_SYSTEM_HPP

#include <linux/limits.h>
#include <cstdint>
#include <cstdlib>
#include <filesystem>
#include <stdexcept>
#include <string_view>
#if defined(__linux__)
#include <stdlib.h>
#include <unistd.h>
#elif defined(_WIN32)
#include <stdlib.h>
#include <process.h>
#else
#error Platform is unsupported
#endif

namespace tc {
  inline std::filesystem::path get_socket_dir();
  inline uint32_t get_pid();

#if defined(__linux__)
  
  inline std::filesystem::path get_socket_dir() {
    using namespace std::string_view_literals;
    
    const char* runtime_dir = getenv("XDG_RUNTIME_DIR");
    if (runtime_dir == nullptr)
      throw std::runtime_error("XDG_RUNTIME_DIR not set!");
    
    auto runtime_dir_path = std::filesystem::path(runtime_dir);
    auto path = std::filesystem::path(runtime_dir) / "TinCan.NET"sv;
    
    std::filesystem::create_directory(path, runtime_dir_path);
    if (!std::filesystem::is_directory(path))
      throw std::runtime_error("Unable to create socket directory");
    
    return path;
  }
  
  inline uint32_t get_pid() {
    return (uint32_t) getpid();
  }
#elif defined(_WIN32)
  inline std::filesystem::path get_socket_dir() {
    using namespace std::string_view_literals;
    
    const char* runtime_dir = getenv("TEMP");
    if (runtime_dir == nullptr)
      throw std::runtime_error("TEMP not set!");
    
    auto runtime_dir_path = std::filesystem::path(runtime_dir);
    auto path = std::filesystem::path(runtime_dir) / "TinCan.NET"sv;
    
    std::filesystem::create_directory(path, runtime_dir_path);
    if (!std::filesystem::is_directory(path))
      throw std::runtime_error("Unable to create socket directory");
    
    return path;
  }
  
  inline uint32_t get_pid() {
    return (uint32_t) _getpid();
  }
#endif
}

#endif