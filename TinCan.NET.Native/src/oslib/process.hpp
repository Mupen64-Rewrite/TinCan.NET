#ifndef TC_PROCESS_HPP
#define TC_PROCESS_HPP

#include <cstdint>
#include <initializer_list>
#include <optional>
#include "common.hpp"
#if defined(OSLIB_OS_WINDOWS)

#elif defined(OSLIB_OS_POSIX)
  #include <unistd.h>
  #include <sys/types.h>
#endif

namespace tc {
  // Lightweight process manager.
  class process {
  public:
    process(std::string_view file, std::initializer_list<std::string_view> args = {});
    
    bool alive();
    
    int join();
    int exit_code();
  private:
#if defined(OSLIB_OS_WINDOWS)
      
#elif defined(OSLIB_OS_POSIX)
    void update_ecode(int waitpid_opts);
    
    pid_t m_pid;
    int m_ecode;
#endif
  };
}

#endif