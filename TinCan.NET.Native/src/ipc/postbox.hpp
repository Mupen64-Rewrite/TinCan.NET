#ifndef TC_IPC_POSTBOX_HPP
#define TC_IPC_POSTBOX_HPP

#include <unordered_map>
#include "../tl/function_ref.hpp"
#include "safe_queue.hpp"
#include <zmq.hpp>

namespace tc {
  class postbox {
  public:
    postbox(const char* uri) { m_sock.bind(uri); }

  private:
    zmq::socket_t m_sock;
    tc::safe_queue<zmq::message_t> m_to_send;
    std::unordered_map<
      std::string, tl::function_ref<void(const zmq::message_t&)>>
      m_recv_handlers;
  };
}  // namespace tc

#endif