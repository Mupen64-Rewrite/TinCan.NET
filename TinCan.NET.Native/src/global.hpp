//
// Created by jgcodes on 11/06/23.
//

#ifndef TC_GLOBAL_HPP_INCLUDED
#define TC_GLOBAL_HPP_INCLUDED

#include <fmt/core.h>
#include <mupen64plus/m64p_types.h>
#include "ipc/postbox.hpp"
#include "util/fs_helper.hpp"
#include <boost/process.hpp>
#include <optional>
#include <stop_token>
#include <string_view>
#include <thread>

namespace tc {
  
  extern m64p_dynlib_handle g_core_handle;
  extern void* g_log_context;
  extern void (*g_log_callback)(void* context, int level, const char* str);
  
  extern std::optional<tc::tempdir_handle> g_tempdir;
  extern std::optional<postbox> g_postbox;
  
  extern std::optional<std::jthread> g_post_thread;
  extern std::optional<boost::process::child> g_process;
  
  void post_thread_loop(std::stop_token tok);
  
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
