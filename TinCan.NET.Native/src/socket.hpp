#ifndef TINCAN_SOCKET_HPP
#define TINCAN_SOCKET_HPP

#include <cstdint>
#include <cstring>
#include <string_view>
#include <msgpack.hpp>
#include <msgpack/v3/sbuffer_decl.hpp>
#include <msgpack/v3/unpack_decl.hpp>
#include <zmq.hpp>

namespace tc {
  inline zmq::context_t& zmq_context() {
    static zmq::context_t context;
    return context;
  }
  
  
  class client_socket {
  public:
    client_socket(std::string_view path) : socket(zmq_context(), zmq::socket_type::req), request_id(0) {}
    
    template <class... Ts>
    msgpack::object_handle send_request(std::string_view method, Ts&&... args) {
      // pack to sbuffer
      msgpack::sbuffer buffer;
      {
        msgpack::packer<msgpack::sbuffer> packer(buffer);
        packer.pack_array(2 + sizeof...(Ts));
        packer.pack(method);
        packer.pack(request_id);
        (packer.pack(std::forward<Ts>(args)), ...);
      }
      
      // send and wait for response
      zmq::message_t msg(buffer.size());
      socket.send(zmq::const_buffer(buffer.data(), buffer.size()));
      auto res = socket.recv(msg, zmq::recv_flags::none);
      
      return msgpack::unpack((const char*) msg.data(), msg.size());
    }
  private:
    zmq::socket_t socket;
    uint64_t request_id;
  };
}

#endif