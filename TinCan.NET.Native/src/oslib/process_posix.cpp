#include <cerrno>
#include <cstdlib>
#include <cstring>
#include <memory>
#include <optional>
#include <stdexcept>
#include <system_error>
#include "common.hpp"
#include "process.hpp"
#ifdef OSLIB_OS_POSIX

  #include <spawn.h>
  #include <sys/wait.h>

// copies a string_view to a malloc'd char*
static inline char* strdup_sv(std::string_view str) {
  char* res = (char*) malloc(str.size() + 1);
  memcpy(res, str.data(), str.size());
  res[str.size()] = '\0';
  return res;
}

// Typed malloc: allocates an array of num objects of type T.
template <class T>
static inline T* tmalloc(size_t num) {
  return (T*) malloc(num * sizeof(T));
}

static void argv_delete(char* args[]) {
  for (char** p = args; *p != nullptr; p++) {
    free(*p);
  }
  free(args);
}

namespace tc {
  process::process(
    std::string_view file, std::initializer_list<std::string_view> args) :
    m_ecode(-1) {
    auto argv = std::unique_ptr<char*[], decltype(&argv_delete)>(
      tmalloc<char*>(args.size() + 2), argv_delete);
    argv[0] = strdup_sv(file);
    for (auto [p, it] = std::pair(&argv[1], args.begin()); it != args.end();
         p++, it++) {
      *p = strdup_sv(*it);
    }
    argv[args.size() + 1] = nullptr;

    int res = posix_spawn(&m_pid, argv[0], nullptr, nullptr, argv.get(), environ);
    if (res < 0) {
      throw std::system_error(errno, std::generic_category());
    }
  }

  void process::update_ecode(int waitpid_opts) {
    int stat, res;
    // see if the process has stopped
    res = waitpid(m_pid, &stat, waitpid_opts);

    if (res < 0) {
      throw std::system_error(errno, std::generic_category());
    }

    // compute the new return code
    if (WIFEXITED(stat)) {
      m_ecode = WEXITSTATUS(stat);
    }
    else if (WIFSIGNALED(stat)) {
      m_ecode = WTERMSIG(stat) + 128;
    }
  }

  int process::join() {
    while (m_ecode == -1) {
      update_ecode(0);
    }
    return m_ecode;
  }

  int process::exit_code() {
    if (m_ecode == -1) {
      update_ecode(WNOHANG);
    }
    return m_ecode;
  }

  bool process::alive() {
    if (m_ecode == -1) {
      update_ecode(WNOHANG);
    }
    return m_ecode == -1;
  }
}  // namespace tc

#endif