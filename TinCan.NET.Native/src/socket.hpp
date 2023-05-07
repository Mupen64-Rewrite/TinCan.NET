#ifndef TINCAN_SOCKET_HPP
#define TINCAN_SOCKET_HPP

#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <filesystem>
#include <string>
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
    // Creates a client socket with a ZeroMQ URI.
    client_socket(const char* path) : socket(zmq_context(), zmq::socket_type::req), request_id(0) {
      socket.bind(path);
    }
    // Creates a client socket with a ZeroMQ URI.
    client_socket(const std::string& path) : client_socket(path.c_str()) {}
    // Creates a ZeroMQ socket using a domain socket at the given path.
    client_socket(const std::filesystem::path& path) : client_socket(std::string("ipc://") + path.string()) {}
    
    template <class... Ts>
    msgpack::object_handle send_request(std::string_view method, Ts&&... args) {
      // pack to sbuffer
      msgpack::sbuffer buffer;
      {
        msgpack::packer<msgpack::sbuffer> packer(buffer);
        packer.pack_array(3);
        packer.pack(method);
        packer.pack(request_id);
        packer.pack_array(sizeof...(args));
        (packer.pack(std::forward<Ts>(args)), ...);
      }
      
      // send and wait for response (blocking)
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