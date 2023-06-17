#include "secrand.hpp"

#include <exception>
#include <system_error>

#if defined(__unix__) || (defined(__APPLE__) && defined(__MACH__))
#include <fcntl.h>
#include <unistd.h>
namespace tc {
  secure_random_device::secure_random_device() : m_hnd(open("/dev/urandom", O_RDONLY | O_CLOEXEC)) {
    if (m_hnd == -1)
      throw std::system_error(errno, std::system_category());
  }
  secure_random_device::~secure_random_device() {
    if (m_hnd >= 0 && close(m_hnd) == -1)
      std::terminate();
  }
  secure_random_device::result_type secure_random_device::operator()() {
    result_type res;
    read(m_hnd, &res, sizeof(res));
    if (m_hnd == -1)
      throw std::system_error(errno, std::system_category());
    return res;
  }
}
#else
#define NOMINMAX
#include <windows.h>
#include <ntstatus.h>
#include <bcrypt.h>
namespace tc {
  secure_random_device::secure_random_device() : m_hnd() {
  }
  secure_random_device::~secure_random_device() {
  }
  secure_random_device::result_type secure_random_device::operator()() {
  }
}
#endif