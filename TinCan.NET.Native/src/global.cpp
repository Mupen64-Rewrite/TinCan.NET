#include "global.hpp"
#include <mupen64plus/m64p_types.h>
#include "ipc/postbox.hpp"
#include <boost/process.hpp>
#include <bit>
#include <cstdint>
#include <cstring>
#include <optional>

m64p_dynlib_handle tc::g_core_handle;
void* tc::g_log_context;
void (*tc::g_log_callback)(void* context, int level, const char* str);

intptr_t tc::g_main_win_handle;
tc::window_system tc::g_main_win_sys;

std::optional<tc::tempdir_handle> tc::g_tempdir;
std::optional<zmq::context_t> tc::g_zmq_ctx;
std::optional<tc::postbox> tc::g_postbox;

std::optional<std::jthread> tc::g_post_thread;
std::optional<boost::process::child> tc::g_process;

CONTROL* tc::g_control_states = nullptr;
std::array<std::atomic_uint32_t, 4> tc::g_input_states { 0, 0, 0, 0 };

std::string_view tc::to_string(tc::window_system sys) {
  using namespace std::literals;
  
  #define TC_WINDOW_SYSTEM_CASE(name) \
  case tc::window_system::name:     \
    return #name##sv;

  switch (sys) { 
    TC_WINDOW_SYSTEM_CASE(windows)
    TC_WINDOW_SYSTEM_CASE(cocoa)
    TC_WINDOW_SYSTEM_CASE(x11)
    TC_WINDOW_SYSTEM_CASE(wayland)
    default:
      return "?"sv;
  }

  #undef TC_WINDOW_SYSTEM_CASE
}

static void do_log(const msgpack::object& obj) {
  auto args = obj.as<std::tuple<int, std::string>>();
  tc::trace((m64p_msg_level) std::get<0>(args), std::get<1>(args));
}
static void do_update_controls(const msgpack::object& obj) {
  auto args = obj.as<std::tuple<int, bool, int, int, int>>();
  auto& state = tc::g_control_states[std::get<0>(args)];
  state.Present = std::get<1>(args);
  state.RawData = std::get<2>(args);
  state.Plugin = std::get<3>(args);
  state.Type = std::get<4>(args);
}
static void do_client_ping(const msgpack::object& obj) {
  tc::g_postbox->enqueue("ClientPingReply");
}

void tc::setup_post_listeners() {
  g_postbox->listen("Log", do_log);
  g_postbox->listen("UpdateControls", do_update_controls);
  g_postbox->listen("ClientPing", do_client_ping);
}

void tc::post_thread_loop(std::stop_token tok) {
  do {
    g_postbox->event_loop(tok);
  } while (!tok.stop_requested());
}