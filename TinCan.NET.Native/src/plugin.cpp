#include <mupen64plus/m64p_plugin.h>
#include <cstdint>
#include <filesystem>
#include <string>
#include <vector>
#include "mupen64plus/m64p_types.h"
#include "oslib/process.hpp"
#include "socket.hpp"
#include "system.hpp"

const char plugin_name[] = "TinCan.NET";

tc::client_socket* p_socket = nullptr;
tc::process* proc = nullptr;
std::filesystem::path socket_path;

CONTROL* ctrls;

static void RequestUpdateControllers() {
  
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
  return 0;
}
EXPORT void CALL RomClosed(void) {
}
EXPORT void CALL InitiateControllers(CONTROL_INFO ControlInfo) {
  ctrls = ControlInfo.Controls;
  
}
EXPORT void CALL ControllerCommand(int Control, unsigned char* Command) {}
EXPORT void CALL GetKeys(int Control, BUTTONS* Keys) {
  
}
EXPORT void CALL ReadController(int Control, unsigned char* Command) {}
EXPORT void CALL SDL_KeyDown(int keymod, int keysym) {}
EXPORT void CALL SDL_KeyUp(int keymod, int keysym) {}
