#ifndef TINCAN_SOCKET_HPP
#define TINCAN_SOCKET_HPP

#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <exception>
#include <filesystem>
#include <stdexcept>
#include <string>
#include <string_view>
#include <type_traits>
#include <fmt/core.h>
#include <msgpack.hpp>
#include <msgpack/v3/object_fwd_decl.hpp>
#include <msgpack/v3/unpack_decl.hpp>
#include <zmq.hpp>

namespace tc {
  // represents a socket error.
  class socket_error : std::runtime_error {
  public:
    using std::runtime_error::runtime_error;
  };
  
  // represents malformed protocol messages and/or errors on the receiving side.
  class protocol_error : std::runtime_error {
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
    // Sends a request to the bound REQ socket.
    template <class R, class... Ts>
    R send_request(std::string_view method, Ts&&... args) requires requires(const msgpack::object& a, R& x) {
      a >> x;
    } {
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
      auto recv_res = socket.recv(msg, zmq::recv_flags::none);
      if (!recv_res)
        throw ::tc::socket_error("");
      
      // unpack msgpack object
      {
        namespace mp = msgpack;
        namespace mpt = msgpack::type;
        auto root_obj = mp::unpack((const char*) msg.data(), msg.size());
        
        // ensure a 2-element array
        if (root_obj->type != mpt::object_type::ARRAY)
          throw protocol_error("Expected 2-element array (data type was not array)");
        auto& root_arr = root_obj->via.array;
        if (root_arr.size != 2)
          throw protocol_error("Expected 2-element array (array was not 2 elements long)");
          
        // check first object's type
        switch (root_arr.ptr[0].type) {
        case mpt::object_type::NIL: {
          // success, return result
          try {
            return root_arr.ptr[1].as<R>();
          }
          catch (const mp::type_error& err) {
            std::throw_with_nested(protocol_error("Receiver returned invalid type for request"));
          }
        } break;
        case mpt::object_type::STR: {
          // error, throw
          auto err_type = root_arr.ptr[0].as<std::string_view>();
          auto err_msg = root_arr.ptr[1].as<std::string_view>();
          throw tc::protocol_error(fmt::format("Receiver threw {}: {}", err_type, err_msg));
        } break;
        default: {
          throw tc::protocol_error("Expected either nil or str at obj[0]");
        } break;
        }
      }
    }
  private:
    zmq::socket_t socket;
  };
  
  
}

#endif