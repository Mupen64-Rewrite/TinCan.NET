#include "fs_helper.hpp"
#include <filesystem>


#if defined(__unix__) || (defined(__APPLE__) && defined(__MACH__))

#include <dlfcn.h>
std::filesystem::path tc::get_own_path() {
  Dl_info dli;
  dladdr(reinterpret_cast<void*>(&tc::get_own_path), &dli);
  return std::filesystem::absolute(std::filesystem::path(dli.dli_fname));
}

#elif defined(_WIN32)

#include <windows.h>
std::filesystem::path tc::get_own_path() {
  throw 0;
}

#endif