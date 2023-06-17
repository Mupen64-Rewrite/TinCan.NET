#ifndef TC_UTIL_TEMPFILE_HPP
#define TC_UTIL_TEMPFILE_HPP

#include <fcntl.h>
namespace tc {
  class exec_tempfile {
  public:
    exec_tempfile() {
      
    }
  private:
    int fd;
    
  };
}

#endif