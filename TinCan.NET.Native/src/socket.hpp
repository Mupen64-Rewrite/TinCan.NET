#ifndef TINCAN_SOCKET_HPP
#define TINCAN_SOCKET_HPP

#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <filesystem>
#include <optional>
#include <sstream>
#include <stdexcept>
#include <string>
#include <string_view>
#include <tuple>
#include <type_traits>
#include <msgpack.hpp>
#include <msgpack/v3/sbuffer_decl.hpp>
#include <msgpack/v3/unpack_decl.hpp>
#include <zmq.hpp>

namespace tc {
  // represents a socket error.
  class socket_error : std::runtime_error {
  public:
    using std::runtime_error::runtime_error;
  };
  
  inline zmq::context_t& zmq_context() {
    static zmq::context_t context;
    return context;
  }
  
  class client_socket {
  public:
    // Creates a client socket with a ZeroMQ URI.
    client_socket(const char* path) : socket(zmq_context(), zmq::socket_type::req) {
      socket.set(zmq::sockopt::req_correlate, true);
      socket.bind(path);
    }
    // Creates a client socket with a ZeroMQ URI.
    client_socket(const std::string& path) : client_socket(path.c_str()) {}
    // Creates a ZeroMQ socket using a domain socket at the given path.
    client_socket(const std::filesystem::path& path) : client_socket(std::string("ipc://") + path.string()) {}
    
    template <class R, class... Ts>
    R send_request(std::string_view method, Ts&&... args) {
      using namespace std::literals;
      // pack to sbuffer
      msgpack::sbuffer buffer;
      {
        msgpack::packer<msgpack::sbuffer> packer(buffer);
        packer.pack_array(2);
        packer.pack(method);
        packer.pack_array(sizeof...(args));
        (packer.pack(std::forward<Ts>(args)), ...);
      }
      
      // send and wait for response (blocking)
      zmq::message_t msg(buffer.size());
      socket.send(zmq::const_buffer(buffer.data(), buffer.size()));
      auto sock_res = socket.recv(msg, zmq::recv_flags::none);
      if (!sock_res)
        throw ::tc::socket_error("");
      
      // unpack msgpack object and check for errors
      using res_t = std::tuple<std::optional<std::string>, msgpack::object>;
      res_t result = msgpack::unpack((const char*) msg.data(), msg.size())->convert();
      if (auto& err_type = std::get<0>(result); err_type.has_value()) {
        std::ostringstream oss;
        oss << *err_type << ": "sv << std::get<1>(result).as<std::string>();
      }
      
      // Convert to expected result type
      if constexpr (!std::is_void_v<R>)
        return std::get<1>(result).as<R>();
      else
        return;
    }
  private:
    zmq::socket_t socket;
  };
  
  
}

#endif