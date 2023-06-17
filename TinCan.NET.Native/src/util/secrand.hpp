#ifndef OSLIB_SECRAND_HPP
#define OSLIB_SECRAND_HPP


#include <cstdint>
#include <span>
#include <random>
#include <system_error>


namespace tc {
  class secure_random_device {
  public:
    using result_type = uint32_t;
  
    secure_random_device();
    
    ~secure_random_device();
      
    secure_random_device(const secure_random_device&) = delete;
    secure_random_device& operator=(const secure_random_device&) = delete;
    
    inline secure_random_device(secure_random_device&& rhs) : m_hnd(rhs.m_hnd)  {
      rhs.m_hnd = -1;
    }
    inline secure_random_device& operator=(secure_random_device&& rhs) {
      m_hnd = rhs.m_hnd;
      rhs.m_hnd = -1;
      return *this;
    }
    
    // Fetch a random integer.
    result_type operator()();
    
    static constexpr result_type min() {
      return std::numeric_limits<result_type>::min();
    }
    
    static constexpr result_type max() {
      return std::numeric_limits<result_type>::max();
    }
  private:
#if defined(__linux__) || defined(__MACH__)
    int m_hnd;
#elif defined(_WIN32)
    BCRYPT_ALG_HANDLE m_hnd;
#endif
  };
}

#if 0
#define NOMINMAX
#include <windows.h>
#include <ntstatus.h>
#include <bcrypt.h>
#include <bit>
#include <limits>

#undef min
#undef max

namespace tc {
  class secure_random_device {
  public:
    using result_type = uint32_t;

    secure_random_device() : m_hnd(get_bcrypt_alg()) {}
    secure_random_device(const secure_random_device&) = delete;
    
    ~secure_random_device() {
      BCryptCloseAlgorithmProvider(m_hnd, 0);
    }
    
    // Uses BCryptGenRandom to generate a random number.
    result_type operator()() { 
      std::array<uint8_t, sizeof(result_type)> bytes;
      BCryptGenRandom(m_hnd, &bytes[0], sizeof(result_type), 0);
      return std::bit_cast<result_type>(bytes);
    }
    static constexpr result_type min() {
      return std::numeric_limits<result_type>::min();
    }
    static constexpr result_type max() { 
      return std::numeric_limits<result_type>::max();
    }
  private:
    static BCRYPT_ALG_HANDLE get_bcrypt_alg() {
      static BCRYPT_ALG_HANDLE res = nullptr;
      if (res == nullptr) {
        auto status = BCryptOpenAlgorithmProvider(
          &res, MS_PRIMITIVE_PROVIDER, nullptr, 0
        );

        switch (status) { 
          case STATUS_SUCCESS:
            break;
          case STATUS_NOT_FOUND:
          case STATUS_INVALID_PARAMETER:
          case STATUS_NO_MEMORY: {
            throw std::runtime_error("BCryptOpenAlgorithmProvider failed");
          } break;
        }
      }
      return res;
    }
    BCRYPT_ALG_HANDLE m_hnd;
  };
}
#endif

#endif