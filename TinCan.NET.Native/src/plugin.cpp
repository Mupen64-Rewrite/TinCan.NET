#include <mupen64plus/m64p_plugin.h>
#include <cstdint>
#include <filesystem>
#include <string>
#include "mupen64plus/m64p_types.h"
#include "oslib/process.hpp"
#include "socket.hpp"
#include "system.hpp"

const char plugin_name[] = "TinCan.NET";

tc::client_socket* p_socket = nullptr;
tc::process* proc = nullptr;
std::filesystem::path socket_path;

EXPORT m64p_error CALL PluginStartup(
  m64p_dynlib_handle core_handle, void* debug_context,
  void (*debug_callback)(void*, int, const char*)) {
  if (p_socket != nullptr)
    return M64ERR_ALREADY_INIT;

  try {
    auto socket_dir = tc::get_socket_dir();
    socket_path = socket_dir / ("ipc-socket-" + std::to_string(tc::get_pid()) + ".sock");
    
    p_socket = new tc::client_socket(socket_path);
    
    // TODO summon server process
  }
  catch (...) {
    return M64ERR_INTERNAL;
  }
  return M64ERR_SUCCESS;
}
EXPORT m64p_error CALL PluginShutdown(void) {
  if (p_socket == nullptr)
    return M64ERR_NOT_INIT;
    
  p_socket->send_request<void>("shutdown");

  try {
    delete p_socket;
    p_socket = nullptr;
    
    if (proc->alive())
      proc->join();
    
    std::filesystem::remove(socket_path);
  }
  catch (...) {
    return M64ERR_INTERNAL;
  }

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
  return p_socket->send_request<bool>("RomOpen");
}
EXPORT void CALL RomClosed(void) {
  p_socket->send_request<void>("RomClosed");
}
EXPORT void CALL ControllerCommand(int Control, unsigned char* Command) {}
EXPORT void CALL GetKeys(int Control, BUTTONS* Keys) {}
EXPORT void CALL InitiateControllers(CONTROL_INFO ControlInfo) {}
EXPORT void CALL ReadController(int Control, unsigned char* Command) {}
EXPORT void CALL SDL_KeyDown(int keymod, int keysym) {}
EXPORT void CALL SDL_KeyUp(int keymod, int keysym) {}
