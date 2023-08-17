//
// Created by jgcodes on 11/06/23.
//

#ifndef TC_GLOBAL_HPP_INCLUDED
#define TC_GLOBAL_HPP_INCLUDED

#include <fmt/core.h>
#include <mupen64plus/m64p_types.h>
#include "ipc/postbox.hpp"
#include "mupen64plus/m64p_plugin.h"
#include "util/fs_helper.hpp"
#include <boost/process.hpp>
#include <zmq.hpp>
#include <array>
#include <atomic>
#include <optional>
#include <stop_token>
#include <string_view>
#include <thread>

namespace tc {
  enum class window_system : uint32_t {
    windows = 0,
    cocoa,
    x11,
    wayland
  };

  
  // Core handles
  extern m64p_dynlib_handle g_core_handle;
  extern void* g_log_context;
  extern void (*g_log_callback)(void* context, int level, const char* str);

  // Frontend handles
  extern intptr_t g_main_win_handle;
  extern window_system g_main_win_sys;
  
  // IPC objects
  extern std::optional<tc::tempdir_handle> g_tempdir;
  extern std::optional<zmq::context_t> g_zmq_ctx;
  extern std::optional<postbox> g_postbox;
  
  // Execution objects
  extern std::optional<std::jthread> g_post_thread;
  extern std::optional<boost::process::child> g_process;
  
  // State objects
  extern CONTROL* g_control_states;
  extern std::array<std::atomic_uint32_t, 4> g_input_states;
  
  std::string_view to_string(tc::window_system sys);
  void post_thread_loop(std::stop_token tok);
  void setup_post_listeners();
  
  inline void trace(m64p_msg_level lvl, const char* str) {
    g_log_callback(g_log_context, lvl, str);
  }
  inline void trace(m64p_msg_level lvl, const std::string& str) {
    g_log_callback(g_log_context, lvl, str.c_str());
  }
  inline void trace(m64p_msg_level lvl, std::string_view str) {
    trace(lvl, std::string(str));
  }
  
  template <class... Ts>
  inline void tracef(m64p_msg_level lvl, fmt::format_string<Ts...> fmt, Ts&&... args) {
    trace(lvl, fmt::format(fmt, std::forward<Ts>(args)...));
  }
}


#endif  // TC_GLOBAL_HPP_INCLUDED
