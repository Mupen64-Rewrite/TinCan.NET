#include <mupen64plus/m64p_plugin.h>
#include <cstdint>
#include <filesystem>
#include <string>
#include <vector>
#include "mupen64plus/m64p_types.h"
#include "socket.hpp"

const char plugin_name[] = "TinCan.NET";

struct gui_data {
  std::filesystem::path fs_path;
  tc::client_socket socket;
}* data;

CONTROL* ctrls;
BUTTONS btns[4];

static void RequestUpdateControllers() {
  auto status = data->socket.send_request<std::array<tc::api::control_status, 4>>("UpdateControllers");
  for (size_t i = 0; i < 4; i++) {
    memcpy(&ctrls[i], &status[i].first, sizeof(CONTROL));
    btns[i].Value = status[i].second.Value;
  }
}

EXPORT m64p_error CALL PluginStartup(
  m64p_dynlib_handle core_handle, void* debug_context,
  void (*debug_callback)(void*, int, const char*)) {
  
}
EXPORT m64p_error CALL PluginShutdown(void) {
  
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
  return data->socket.send_request<bool>("RomOpen");
}
EXPORT void CALL RomClosed(void) {
  data->socket.send_request<void>("RomOpen");
}
EXPORT void CALL InitiateControllers(CONTROL_INFO ControlInfo) {
  ctrls = ControlInfo.Controls;
  RequestUpdateControllers();
}
EXPORT void CALL ControllerCommand(int Control, unsigned char* Command) {}
EXPORT void CALL GetKeys(int Control, BUTTONS* Keys) {
  // updating all control statuses on each query is very inefficient, but it shouldn't be that bad
  RequestUpdateControllers();
  Keys->Value = btns[Control].Value;
}
EXPORT void CALL ReadController(int Control, unsigned char* Command) {}
EXPORT void CALL SDL_KeyDown(int keymod, int keysym) {
  data->socket.send_request<void>("SDL_KeyDown", keymod, keysym);
}
EXPORT void CALL SDL_KeyUp(int keymod, int keysym) {
  data->socket.send_request<void>("SDL_KeyUp", keymod, keysym);
}
