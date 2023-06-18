
#include <mupen64plus/m64p_common.h>
#include <mupen64plus/m64p_plugin.h>
#include <mupen64plus/m64p_types.h>

#include "global.hpp"
#include "ipc/postbox.hpp"
#include "util/fs_helper.hpp"
#include <boost/process.hpp>
#include <boost/process/io.hpp>

#define TC_IF_NOT_NULL(ptr) \
  if (ptr)                  \
  *ptr
#define TC_EXPORT(T) extern "C" EXPORT T CALL

#ifdef _WIN32
  #define TC_EXECUTABLE_EXT ".exe"
  #define TC_NEWLINE "\r\n"
#else
  #define TC_EXECUTABLE_EXT
  #define TC_NEWLINE "\n"
#endif

TC_EXPORT(m64p_error)
PluginStartup(
  m64p_dynlib_handle core_handle, void* log_context,
  void (*log_callback)(void* context, int level, const char* str)) {
    
  if (tc::g_postbox.has_value())
    return M64ERR_ALREADY_INIT;
  
  tc::g_core_handle  = core_handle;
  tc::g_log_context  = log_context;
  tc::g_log_callback = log_callback;

  try {
    // init temp dir
    tc::g_tempdir.emplace();
    auto sock_path = tc::g_tempdir->operator*() / "socket.sock";
    
    // init socket
    tc::g_zmq_ctx.emplace();
    tc::g_postbox.emplace(*tc::g_zmq_ctx, fmt::format("ipc://{}", sock_path.string()));
    auto conn_ep = tc::g_postbox->endpoint();
    tc::tracef(M64MSG_VERBOSE, "Using endpoint {}", conn_ep);
    
    // prep handlers and start postbox
    tc::g_postbox->listen("Log", [](const msgpack::object& obj) {
      auto args = obj.as<std::tuple<int, std::string>>();
      tc::trace((m64p_msg_level) std::get<0>(args), std::get<1>(args));
    });
    tc::g_post_thread.emplace(tc::post_thread_loop);
    
    // find path to desired executable
    auto exe_path =
      tc::get_own_path().parent_path() / "TinCan.NET" TC_EXECUTABLE_EXT;
    tc::g_process.emplace(exe_path.c_str(), boost::process::args({conn_ep}));
  }
  catch (const std::exception& exc) {
    fmt::print("Caught exception: {}" TC_NEWLINE, exc.what());
    return M64ERR_SYSTEM_FAIL;
  }
  catch (...) {
    return M64ERR_SYSTEM_FAIL;
  }

  return M64ERR_SUCCESS;
}

TC_EXPORT(m64p_error) PluginShutdown() {
  using namespace std::literals;
  if (!tc::g_postbox.has_value())
    return M64ERR_NOT_INIT;
  
  tc::trace(M64MSG_VERBOSE, "Signaling shutdown");
  tc::g_postbox->enqueue("Shutdown"sv);
  
  tc::trace(M64MSG_VERBOSE, "Awaiting shutdown");
  if (tc::g_process.has_value() && tc::g_process->joinable())
    tc::g_process->join();
  
  tc::trace(M64MSG_VERBOSE, "Cleaning up");
  
  tc::g_post_thread->request_stop();
  tc::g_post_thread->join();
  tc::g_post_thread.reset();

  tc::g_postbox.reset();
  tc::g_zmq_ctx.reset();
  
  tc::trace(M64MSG_VERBOSE, "Shutdown complete");
  return M64ERR_SUCCESS;
}

TC_EXPORT(m64p_error)
PluginGetVersion(
  m64p_plugin_type* plugin_type, int* plugin_version, int* api_version,
  const char** plugin_name_ptr, int* caps) {
  TC_IF_NOT_NULL(plugin_type)     = M64PLUGIN_INPUT;
  TC_IF_NOT_NULL(plugin_version)  = 0x000100;
  TC_IF_NOT_NULL(api_version)     = 0x020101;
  TC_IF_NOT_NULL(plugin_name_ptr) = "TinCan.NET";
  TC_IF_NOT_NULL(caps)            = 0;
  return M64ERR_SUCCESS;
}

TC_EXPORT(int) RomOpen() {
  return true;
}

TC_EXPORT(void) RomClosed() {
}

TC_EXPORT(void) InitiateControllers() {}

TC_EXPORT(void) GetKeys(int index, BUTTONS* keys) {
  keys->Value = 0;
}

TC_EXPORT(void) ControllerCommand(int index, unsigned char* data) {}

TC_EXPORT(void) ReadController(int index, unsigned char* data) {}

TC_EXPORT(void) SDL_KeyDown(int modifiers, int scancode) {}

TC_EXPORT(void) SDL_KeyUp(int modifiers, int scancode) {}