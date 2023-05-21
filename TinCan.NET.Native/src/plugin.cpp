#include <mupen64plus/m64p_plugin.h>
#include <cstdint>
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

CONTROL* ctrls;
BUTTONS btns[4];

static void RequestUpdateControllers() {
  auto status = g_sock->send_request<std::array<tc::api::control_status, 4>>(
    "UpdateControllers");
  for (size_t i = 0; i < 4; i++) {
    memcpy(&ctrls[i], &status[i].first, sizeof(CONTROL));
    btns[i].Value = status[i].second.Value;
  }
}

EXPORT m64p_error CALL PluginStartup(
  m64p_dynlib_handle core_handle, void* debug_context,
  void (*debug_callback)(void*, int, const char*)) {
  try {
    g_tmpdir       = new oslib::tempdir_handle();
    auto sock_path = **g_tmpdir / "ipc.sock";
    g_sock         = new tc::client_socket(sock_path);

    auto self_dir = oslib::get_own_path().parent_path();

    g_proc = new oslib::process(
      (self_dir / TC_EXECUTABLE_NAME).string(), {sock_path.string()});
  }
  catch (...) {
    if (g_tmpdir != nullptr) {
      delete g_tmpdir;
      g_tmpdir = nullptr;
    }
    if (g_sock != nullptr) {
      delete g_sock;
      g_sock = nullptr;
    }
    return M64ERR_INTERNAL;
  }
  return M64ERR_SUCCESS;
}
EXPORT m64p_error CALL PluginShutdown(void) {
  if (!g_tmpdir)
    return M64ERR_NOT_INIT;

  delete g_tmpdir;
  g_tmpdir = nullptr;
  delete g_sock;
  g_sock = nullptr;

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
  return g_sock->send_request<bool>("RomOpen");
}
EXPORT void CALL RomClosed(void) {
  g_sock->send_request<void>("RomClosed");
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
