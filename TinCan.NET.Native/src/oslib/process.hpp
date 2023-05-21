#ifndef OSLIB_PROCESS_HPP_INCLUDED
#define OSLIB_PROCESS_HPP_INCLUDED

#include <algorithm>
#include <cstdlib>
#include <initializer_list>
#include <memory>
#include <optional>
#include <stdexcept>
#include <string_view>

namespace oslib {
  class process;
}

#if defined(__linux__) || defined(__MACH__)
  #pragma region POSIX implementation
  #include <sys/wait.h>
  #include <unistd.h>
  #include <spawn.h>

  #include <array>
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
      
      // allocate argv tables
      size_t total_chars = program.length() + 1;
      for (std::string_view a : args)
        total_chars += a.length() + 1;

      auto argstrs = std::make_unique<char[]>(total_chars);
      auto argv    = std::make_unique<char*[]>(2 + args.size());

      // copy arguments in
      char* ps  = argstrs.get();
      char** pp = argv.get();

      // copy in argv[0]
      *pp++ = ps;
      ps    = std::copy(program.begin(), program.end(), ps);
      *ps++ = '\0';
      // copy in argv[1:]
      for (std::string_view a : args) {
        *pp++ = ps;
        ps    = std::copy(a.begin(), a.end(), ps);
        *ps++ = '\0';
      }
      posix_spawn(&pid, argv[0], nullptr, nullptr, argv.get(), environ);
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
      }
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
      if (rc == notexit)
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

      STARTUPINFOW startupInfo;
      GetStartupInfoW(&startupInfo);

      BOOL res = CreateProcessW(
        wsAppName.c_str(), wsCmdLine.c_str(), nullptr, nullptr, true,
        CREATE_NO_WINDOW, nullptr, nullptr, &startupInfo, &proc_info);

      if (!res) {
        throw std::system_error(GetLastError(), std::system_category());
      }
    }

    bool joinable() {
      DWORD maybe_rc;
      BOOL res = GetExitCodeProcess(proc_info.hProcess, &maybe_rc);
      if (maybe_rc == STATUS_PENDING) {
        return true;
      }
      else {
        if (!rc.has_value())
          rc = maybe_rc;
        return false;
      }
    }

    // Wait for the process to close. Returns the exit code.
    DWORD join() {
      if (rc.has_value())
        return *rc;

      // wait until the process stops
      DWORD res = WaitForSingleObject(proc_info.hProcess, INFINITE);

      if (res == WAIT_OBJECT_0) {
        DWORD maybe_rc;
        BOOL res2 = GetExitCodeProcess(proc_info.hProcess, &maybe_rc);
        rc        = maybe_rc;

        return *rc;
      }
      else if (res == WAIT_FAILED) {
        throw std::system_error(GetLastError(), std::system_category());
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