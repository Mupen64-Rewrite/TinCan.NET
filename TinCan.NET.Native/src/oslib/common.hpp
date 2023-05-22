#ifndef OSLIB_COMMON_HPP
#define OSLIB_COMMON_HPP

#include <memory>
#include <type_traits>
#include <utility>
#ifdef _MSVC_LANG
  #define OSLIB_CPP_STD _MSVC_LANG
#else
  #define OSLIB_CPP_STD __cplusplus
#endif

#if defined(_WIN32)
  #define OSLIB_OS_WINDOWS
#elif defined(__MACH__)
  #define OSLIB_OS_MACOS
  #define OSLIB_OS_POSIX
#elif defined(__linux__)
  #define OSLIB_OS_LINUX
  #define OSLIB_OS_POSIX
#elif defined(__unix__) || defined(__unix) || defined(unix)
  #undef unix
  #define OSLIB_OS_POSIX
#endif

#include <cstddef>
#include <cstring>
#include <string>
#include <string_view>

namespace tc {
  // Class acting as a generic null-terminated string handle.
  class cstr_handle {
  public:
    cstr_handle(const char* str) : m_ptr(const_cast<char*>(str)), m_should_free(false) {}
    cstr_handle(const std::string& str) :
      m_ptr(const_cast<char*>(str.c_str())), m_should_free(false) {}
    cstr_handle(std::string_view sv) :
      m_ptr(static_cast<char*>(malloc(sv.size() + 1))),
      m_should_free(true) {
      memcpy(const_cast<char*>(m_ptr), sv.data(), sv.size());
      const_cast<char*>(m_ptr)[sv.size()] = '\0';
    }

    cstr_handle(std::nullptr_t = nullptr) :
      m_ptr(nullptr), m_should_free(false) {}

    cstr_handle(const cstr_handle& str) :
      m_ptr(str.m_should_free ? strdup(str.m_ptr) : str.m_ptr),
      m_should_free(str.m_should_free) {}
      
    cstr_handle& operator=(const cstr_handle& str) {
      if (m_should_free)
        free(m_ptr);
      
      m_should_free = str.m_should_free;
      m_ptr = m_should_free? strdup(str.m_ptr) : str.m_ptr;
      return *this;
    }
    
    cstr_handle(cstr_handle&& str) :
      m_ptr(str.m_ptr),
      m_should_free(str.m_should_free) {
      str.m_should_free = false;
    }
    
    cstr_handle& operator=(cstr_handle&& str) {
      if (m_should_free)
        free(m_ptr);
      
      m_should_free = str.m_should_free;
      m_ptr = str.m_ptr;
      return *this;
    }
    

    ~cstr_handle() {
      if (m_should_free)
        free(m_ptr);
    }

    operator const char*() const { return m_ptr; }

  private:
    char* m_ptr;
    bool m_should_free;
  };
  
}  // namespace tc

#endif