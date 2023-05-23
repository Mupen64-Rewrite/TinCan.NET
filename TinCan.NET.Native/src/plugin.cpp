#include <mupen64plus/m64p_plugin.h>
#include <cstdint>
#include <exception>
#include <filesystem>
#include <string>
#include <vector>
#include "mupen64plus/m64p_types.h"
#include "oslib/fsutil.hpp"
#include "oslib/process.hpp"
#include "socket.hpp"

const char plugin_name[] = "TinCan.NET";

#if defined(__linux__)
  #define TC_EXECUTABLE_NAME "TinCan.NET"
#elif defined(_WIN32)
  #define TC_EXECUTABLE_NAME "TinCan.NET.exe"
#else
  #error "UNSUPPORTED PLATFORM"
#endif

oslib::tempdir_handle* g_tmpdir = nullptr;
tc::client_socket* g_sock       = nullptr;
oslib::process* g_proc          = nullptr;

void (*g_debug_callback)(void*, int, const char*);
void* g_debug_context;

CONTROL* ctrls;
BUTTONS btns[4];

char exception_msg[1024];

static inline void RequestUpdateControllers() {
  auto status = g_sock->send_request<std::array<tc::api::control_status, 4>>(
    "UpdateControllers");
  for (size_t i = 0; i < 4; i++) {
    memcpy(&ctrls[i], &status[i].first, sizeof(CONTROL));
    btns[i].Value = status[i].second.Value;
  }
}

static inline void m64p_log(int level, const char* msg) {
  (*g_debug_callback)(g_debug_context, level, msg);
}

static inline void report_exception() {
  try {
    auto exc = std::current_exception();
    if ((bool) exc)
      std::rethrow_exception(exc);
  }
  catch (const std::exception& e) {
    char* p = fmt::format_to(exception_msg, "{}:\n {}", typeid(e).name(), e.what());
    *p = '\0';
    m64p_log(M64MSG_ERROR, exception_msg);
  }
}

extern "C" {
EXPORT m64p_error CALL PluginStartup(
  m64p_dynlib_handle core_handle, void* debug_context,
  void (*debug_callback)(void*, int, const char*)) {
  g_debug_callback = debug_callback;
  g_debug_context = debug_context;
    m64p_log(M64MSG_STATUS, "TinCan.NET started");
    
  try {
    m64p_log(M64MSG_STATUS, "Acquiring temp dir");
    g_tmpdir       = new oslib::tempdir_handle();
    auto sock_path = **g_tmpdir / "ipc.sock";
    m64p_log(M64MSG_STATUS, "Binding socket");
    g_sock         = new tc::client_socket(sock_path);

    auto self_dir = oslib::get_own_path().parent_path();

    m64p_log(M64MSG_STATUS, "Forking process");
    g_proc = new oslib::process(
      (self_dir / TC_EXECUTABLE_NAME).string(), {sock_path.string()});
    m64p_log(M64MSG_STATUS, "Doing a test ping...");
    g_sock->ping();
    m64p_log(M64MSG_STATUS, "Ping returned, setup complete");
  }
  catch (...) {
    return M64ERR_INTERNAL;
  }
  return M64ERR_SUCCESS;
}
EXPORT m64p_error CALL PluginShutdown(void) {
  if (!g_tmpdir)
    return M64ERR_NOT_INIT;
  
  
  // m64p_log(M64MSG_STATUS, "Initiating shutdown");
  g_sock->send_request<void>("Shutdown");
  
  // m64p_log(M64MSG_STATUS, "Awaiting process close");
  g_proc->join();
  
  // m64p_log(M64MSG_STATUS, "Cleaning up");
  delete g_proc;
  g_proc = nullptr;
  delete g_sock;
  g_sock = nullptr;
  delete g_tmpdir;
  g_tmpdir = nullptr;
  
  

  return M64ERR_SUCCESS;
}
EXPORT m64p_error CALL PluginGetVersion(
  m64p_plugin_type* type, int* version, int* api_version, const char** name,
  int* caps) {
  if (type != nullptr)
    *type = M64PLUGIN_INPUT;
  if (version != nullptr)
    *version = 0x000100;
  if (api_version != nullptr)
    *api_version = 0x020100;
  if (name != nullptr)
    *name = plugin_name;
  if (caps != nullptr)
    *caps = 0;

  return M64ERR_SUCCESS;
}

EXPORT int CALL RomOpen(void) {
  return 1;
}
EXPORT void CALL RomClosed(void) {
  
}
EXPORT void CALL InitiateControllers(CONTROL_INFO ControlInfo) {
  ctrls = ControlInfo.Controls;
  // RequestUpdateControllers();
}
EXPORT void CALL ControllerCommand(int Control, unsigned char* Command) {}
EXPORT void CALL GetKeys(int Control, BUTTONS* Keys) {
  // updating all control statuses on each query is very inefficient, but it
  // shouldn't be that bad
  // RequestUpdateControllers();
  // Keys->Value = btns[Control].Value;
}
EXPORT void CALL ReadController(int Control, unsigned char* Command) {}
EXPORT void CALL SDL_KeyDown(int keymod, int keysym) {
  // g_sock->send_request<void>("SDL_KeyDown", keymod, keysym);
}
EXPORT void CALL SDL_KeyUp(int keymod, int keysym) {
  // g_sock->send_request<void>("SDL_KeyUp", keymod, keysym);
}
}