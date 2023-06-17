#include "global.hpp"
#include <mupen64plus/m64p_types.h>
#include <boost/process.hpp>
#include <optional>

m64p_dynlib_handle tc::core_handle;
void* tc::log_context;
void (*tc::log_callback)(void* context, int level, const char* str);
std::optional<boost::process::child> tc::process;