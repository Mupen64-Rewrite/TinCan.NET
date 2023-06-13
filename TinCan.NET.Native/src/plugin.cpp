
#include <mupen64plus/m64p_common.h>
#include <mupen64plus/m64p_plugin.h>
#include <mupen64plus/m64p_types.h>

#include "global.hpp"

#define TC_IF_NOT_NULL(ptr) \
  if (ptr)                  \
  *ptr
#define TC_EXPORT(T) EXPORT T CALL

TC_EXPORT(m64p_error)
PluginStartup(
  m64p_dynlib_handle core_handle, void* log_context,
  void (*log_callback)(void* context, int level, const char* str)) {
  tc::core_handle = core_handle;
  tc::log_context = log_context;
  tc::log_callback = log_callback;


  return M64ERR_SUCCESS;
}

TC_EXPORT(m64p_error) PluginShutdown() {
  return M64ERR_SUCCESS;
}

TC_EXPORT(m64p_error)
PluginGetVersion(
  m64p_plugin_type* plugin_type, int* plugin_version, int* api_version,
  const char** plugin_name_ptr, int* caps) {
  TC_IF_NOT_NULL(plugin_type)     = M64PLUGIN_INPUT;
  TC_IF_NOT_NULL(plugin_version)  = 0x000100;
  TC_IF_NOT_NULL(api_version)     = 0x020000;
  TC_IF_NOT_NULL(plugin_name_ptr) = "TinCan.NET";
  TC_IF_NOT_NULL(caps)            = 0;
  return M64ERR_SUCCESS;
}

TC_EXPORT(int) RomOpen() {
  
  return true;
}

TC_EXPORT(void) RomClosed() {}