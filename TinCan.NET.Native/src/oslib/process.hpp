#ifndef OSLIB_PROCESS_HPP_INCLUDED
#define OSLIB_PROCESS_HPP_INCLUDED

#include <algorithm>
#include <cstdlib>
#include <filesystem>
#include <initializer_list>
#include <memory>
#include <optional>
#include <stdexcept>
#include <string_view>
#include <variant>

namespace oslib {
  class process;
}

#if defined(__linux__) || defined(__MACH__)
  #pragma region POSIX implementation
  #include <sys/wait.h>
  #include <unistd.h>
  #include <fcntl.h>
  #include <spawn.h>

  #include <array>
  #include <iostream>
  #include <cerrno>
  #include <cstddef>
  #include <numeric>
  #include <system_error>
namespace oslib {
  
  class process {
  public:
    process(
      std::string_view program,
      std::initializer_list<std::string_view> args = {}) :
      rc(notexit) {
      // find size for argv tables
      size_t argv_table_len = 1 + args.size() + 1;
      size_t str_table_len = program.size() + 1;
      for (auto& arg : args) {
        argv_table_len += 1;
        str_table_len += arg.length() + 1;
      }
      // allocate tables
      std::unique_ptr<char[]> str_table = std::make_unique<char[]>(str_table_len);
      std::unique_ptr<char*[]> argv_table = std::make_unique<char*[]>(argv_table_len);

      char* pst = &str_table[0];
      char** pat = &argv_table[0];
      // populate program argument
      *pat++ = pst;
      pst = std::copy(program.begin(), program.end(), pst);
      *pst++ = '\0';
      // populate arguments
      for (auto& arg : args) {
        *pat++ = pst;
        pst = std::copy(arg.begin(), arg.end(), pst);
        *pst++ = '\0';
      }
      
      // initializing last few structures
      posix_spawn_file_actions_t psfa;
      posix_spawn_file_actions_init(&psfa);
      
      posix_spawnattr_t psa;
      posix_spawnattr_init(&psa);
      
      int err;
      if ((err = posix_spawn(&pid, argv_table[0], &psfa, &psa, argv_table.get(), environ)) != 0) {
        throw std::system_error(err, std::generic_category());
      }
      
      posix_spawn_file_actions_destroy(&psfa);
      posix_spawnattr_destroy(&psa);
    }

    bool joinable() {
      if (rc != notexit)
        return false;
      
      int stat;
      if (int res = waitpid(pid, &stat, WNOHANG); res == pid) {
        if (WIFEXITED(stat)) {
          rc = WEXITSTATUS(res);
          return true;
        }
        if (WIFSIGNALED(stat)) {
          rc = -WSTOPSIG(stat);
          return true;
        }
        throw std::runtime_error("THIS SHOULDN'T HAPPEN");
      }
      return false;
    }

    // Wait for the process to close if it hasn't. Returns the exit code.
    int join() {
      if (rc != notexit)
        return rc;
      int stat;
      pid_t res;
      do {
        res = waitpid(pid, &stat, 0);
      } while (res != pid);
      if (res == (pid_t) -1) {
        throw std::system_error(res, std::generic_category());
      }
      
      if (WIFEXITED(stat)) {
        rc = WEXITSTATUS(res);
        return rc;
      }
      if (WIFSIGNALED(stat)) {
        rc = -WSTOPSIG(stat);
        return rc;
      }
      throw std::runtime_error("THIS SHOULDN'T HAPPEN");
    }

  private:
    static constexpr int notexit = -1024;
    
    pid_t pid;
    int rc;
  };
}  // namespace oslib
  #pragma endregion
#elif defined(_WIN32)
  #pragma region WinAPI implementation
  #include "details/convert.hpp"

  #ifndef WIN32_LEAN_AND_MEAN
    #define WIN32_LEAN_AND_MEAN
  #endif
  #ifndef _WINSOCKAPI_
    #define _WINSOCKAPI_
  #endif
  #include <windows.h>

namespace oslib {
  class process {
  public:
    process(
      std::string_view program,
      std::initializer_list<std::string_view> args = {}) :
      rc(std::nullopt) {
      // validate file path for app name, see:
      // https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
      for (char c : program) {
        for (char inv : {'<', '>', ':', '"', '/', '\\', '|', '?', '*'}) {
          if (c == inv) {
            throw std::invalid_argument(
              "Program name contains invalid characters (WINDOWS)");
          }
        }
      }
      auto wsAppName = details::to_utf16(program);

      // generate command line to match this spec:
      // https://docs.microsoft.com/en-us/cpp/cpp/main-function-command-line-args?view=msvc-170

      // argv[0]
      std::wostringstream fmt;
      fmt << L'"' << wsAppName << L'"';

      // argv[1:]
      for (std::string_view arg : args) {
        std::wstring argAsWChar = details::to_utf16(arg);
        // this will be a quoted version of argAsWChar
        std::wstring wArg = L" \"";
        wArg.reserve(argAsWChar.size());
        // escape backslashes and quotes
        for (wchar_t wc : argAsWChar) {
          switch (wc) {
            case L'\\':
            case L'"': {
              wArg.push_back(L'\\');
              wArg.push_back(wc);
            } break;
            default: {
              wArg.push_back(wc);
            } break;
          }
        }
        wArg.push_back(L'"');

        fmt << wArg;
      }

      auto wsCmdLine = fmt.str();
      wsCmdLine.push_back('\0');

      STARTUPINFOW startupInfo;
      GetStartupInfoW(&startupInfo);

      BOOL res = CreateProcessW(
        wsAppName.c_str(), wsCmdLine.data(), nullptr, nullptr, true,
        CREATE_NO_WINDOW, nullptr, nullptr, &startupInfo, &proc_info);

      if (!res) {
        throw std::system_error(GetLastError(), std::system_category());
      }
    }

    bool joinable() { 
      if (rc.has_value())
        return false;

      DWORD maybe_rc;
      if (!GetExitCodeProcess(proc_info.hProcess, &maybe_rc)) {
        throw std::system_error(GetLastError(), std::system_category());
      }
      if (maybe_rc == STILL_ACTIVE)
        return true;
      
      rc = maybe_rc;
      return false;
    }

    // Wait for the process to close. Returns the exit code.
    DWORD join() {
      if (rc.has_value())
        return *rc;

      // wait until the process stops
      DWORD res = WaitForSingleObject(proc_info.hProcess, INFINITE);
      switch (res) { 
      case WAIT_OBJECT_0: {
          DWORD maybe_rc;
          if (!GetExitCodeProcess(proc_info.hProcess, &maybe_rc)) {
            throw std::system_error(GetLastError(), std::system_category());
          }
          rc = maybe_rc;
          return *rc;
      } break;
      case WAIT_FAILED: {
          throw std::system_error(GetLastError(), std::system_category());
      } break;
      }
      throw std::runtime_error("Unexpected wait return value");
    }

    // Get the process's exit code, if it has closed.
    // If the process was terminated by SEH exception, returns its code.
    DWORD exit_code() { return rc.value(); }

  private:
    PROCESS_INFORMATION proc_info;
    std::optional<DWORD> rc;
  };
}  // namespace oslib
  #pragma endregion
#endif

#endif