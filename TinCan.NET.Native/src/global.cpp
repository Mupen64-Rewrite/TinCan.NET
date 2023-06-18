#include "global.hpp"
#include <mupen64plus/m64p_types.h>
#include "ipc/postbox.hpp"
#include <boost/process.hpp>
#include <optional>

m64p_dynlib_handle tc::g_core_handle;
void* tc::g_log_context;
void (*tc::g_log_callback)(void* context, int level, const char* str);

std::optional<tc::tempdir_handle> tc::g_tempdir;
std::optional<zmq::context_t> tc::g_zmq_ctx;
std::optional<tc::postbox> tc::g_postbox;

std::optional<std::jthread> tc::g_post_thread;
std::optional<boost::process::child> tc::g_process;

std::optional<std::span<CONTROL, 4>> tc::g_control_states;
std::array<std::atomic_uint32_t, 4> tc::g_input_states { 0, 0, 0, 0 };

void tc::post_thread_loop(std::stop_token tok) {
  do {
    g_postbox->event_loop(tok);
  } while (!tok.stop_requested());
}