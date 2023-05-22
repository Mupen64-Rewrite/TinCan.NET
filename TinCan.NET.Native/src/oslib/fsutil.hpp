#ifndef TC_OSLIB_FSUTIL_HPP
#define TC_OSLIB_FSUTIL_HPP

#include <array>
#include <filesystem>
#include <random>
#include <span>
#include <stdexcept>
#include <utility>
#include "secrand.hpp"

#ifdef _WIN32
#define NOMINMAX
#include <windows.h>
#undef min
#undef max
#endif

namespace oslib {
  // Handle managing a temporary directory.
  class tempdir_handle {
  public:
    tempdir_handle(
      const std::filesystem::path& parent =
        std::filesystem::temp_directory_path()) {
      // make sure the parent directory exists
      if (!std::filesystem::is_directory(parent)) {
        throw std::invalid_argument("Provided path is not a directory");
      }
      // setup template
      std::array<char, 4 + template_len> tmpdir_name;
      tmpdir_name[0]     = 't';
      tmpdir_name[1]     = 'm';
      tmpdir_name[2]     = 'p';
      tmpdir_name.back() = '\0';
    
      for (int tries = 100; tries > 0; tries--) {
        // generate a name
        insert_temp_chars(std::span<char, 10>(&tmpdir_name[3], 10));
        m_dir_path = parent / tmpdir_name.data();
        // if it's OK, use it
        if (!std::filesystem::exists(m_dir_path)) {
          std::filesystem::create_directory(m_dir_path);
          return;
        }
      }
    }

    tempdir_handle(const tempdir_handle&)            = delete;
    tempdir_handle& operator=(const tempdir_handle&) = delete;

    tempdir_handle(tempdir_handle&& rhs) {
      using std::swap;
      swap(m_dir_path, rhs.m_dir_path);
    }
    tempdir_handle& operator=(tempdir_handle&& rhs) {
      using std::swap;
      // remove old path
      if (!m_dir_path.empty()) {
        std::filesystem::remove_all(m_dir_path);
      }
      m_dir_path.clear();
      // swap in new path, causing old path to be swapped to RHS
      swap(m_dir_path, rhs.m_dir_path);
      return *this;
    }

    ~tempdir_handle() {
      // delete the directory in question if needed
      if (!m_dir_path.empty()) {
        std::filesystem::remove_all(m_dir_path);
      }
    }
    
    const std::filesystem::path& operator*() {
      return m_dir_path;
    }

  private:
    std::filesystem::path m_dir_path;
#if defined(__linux__)
    static constexpr char temp_name_valid_chars[] = {
      'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
      'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
      'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
      'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
      '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
    };
#elif defined(_WIN32) || defined(__MACH__)
    static constexpr char temp_name_valid_chars[] = {
      'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
      'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
      'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
    };
#endif
    static constexpr size_t template_len = 10;
    void insert_temp_chars(std::span<char, template_len> span) {
      secure_random_device rd;
      std::uniform_int_distribution<size_t> rng(
        0, sizeof(temp_name_valid_chars) - 1);
      for (size_t i = 0; i < span.size(); i++) {
        span[i] = temp_name_valid_chars[rng(rd)];
      }
    }
  };
  
#if defined(__linux__)
  inline std::filesystem::path get_own_path() {
    using namespace std::literals;
    auto path = std::filesystem::read_symlink("/proc/self/exe"sv);
    if (!std::filesystem::exists(path))
      throw std::runtime_error("Can't find own path");
    
    return std::filesystem::canonical(path);
  }
#elif defined(_WIN32)
  inline std::filesystem::path get_own_path() {
    wchar_t path[MAX_PATH];
    if (GetModuleFileNameW(nullptr, path, MAX_PATH) == 0) {
      throw std::system_error(GetLastError(), std::system_category());
    }
    return path;
  }
#endif
}  // namespace oslib

#endif