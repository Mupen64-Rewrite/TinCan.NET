#include <cstdint>
#include <mupen64plus/m64p_plugin.h>
#include <msgpack.hpp>
#include <zmq.hpp>

EXPORT m64p_error CALL
PluginStartup(m64p_dynlib_handle, void*, void (*)(void*, int, const char*)) {
  return M64ERR_SUCCESS;
}
EXPORT m64p_error CALL PluginShutdown(void) {
  return M64ERR_SUCCESS;
}
EXPORT m64p_error CALL
PluginGetVersion(m64p_plugin_type* type, int* version, int* api_version, const char** name, int* caps) {
  uint32_t send_bits = 0;
  
  // OR in the corresponding bits for paramters
  send_bits |= uint32_t(type != nullptr) << 0;
  send_bits |= uint32_t(version != nullptr) << 1;
  send_bits |= uint32_t(api_version != nullptr) << 2;
  send_bits |= uint32_t(name != nullptr) << 3;
  send_bits |= uint32_t(caps != nullptr) << 4;
  
  
  
  return M64ERR_SUCCESS;
}


EXPORT int CALL RomOpen(void) {
  return M64ERR_SUCCESS;
}
EXPORT void CALL RomClosed(void) {}
EXPORT void CALL ControllerCommand(int Control, unsigned char* Command) {}
EXPORT void CALL GetKeys(int Control, BUTTONS* Keys) {}
EXPORT void CALL InitiateControllers(CONTROL_INFO ControlInfo) {}
EXPORT void CALL ReadController(int Control, unsigned char* Command) {}
EXPORT void CALL SDL_KeyDown(int keymod, int keysym) {}
EXPORT void CALL SDL_KeyUp(int keymod, int keysym) {}
