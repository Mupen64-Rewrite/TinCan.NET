#ifndef TINCAN_SOCKET_HPP
#define TINCAN_SOCKET_HPP

#include <fmt/core.h>
#include <chrono>
#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <exception>
#include <filesystem>
#include <stdexcept>
#include <string>
#include <string_view>
#include <type_traits>
#include "mp_check.hpp"
#include "api.hpp"
#include <msgpack.hpp>
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
    client_socket(const char* path) :
      m_socket(zmq_context(), zmq::socket_type::req) {
      m_socket.bind(path);
      // ensure message integrity
      m_socket.set(zmq::sockopt::req_correlate, true);
      // wait at most 0.05 s for a response
      m_socket.set(zmq::sockopt::rcvtimeo, 50);
    }
    // Creates a client socket with a ZeroMQ URI.
    client_socket(const std::string& path) : client_socket(path.c_str()) {}
    // Creates a ZeroMQ socket using a domain socket at the given path.
    client_socket(const std::filesystem::path& path) :
      client_socket(std::string("ipc://") + path.string()) {}
    // Sends a request to the bound REQ socket. Requires all types to be serializable.
    template <class R, class... Ts>
      // requires(
      //   (std::is_void_v<R> || is_msgpack_serializable<R>::value) &&
      //   (true && ... && is_msgpack_serializable<Ts>::value) )
    [[nodiscard]] R send_request(std::string_view method, Ts&&... args) {
      using namespace std::literals;
      namespace mp  = msgpack;
      namespace mpt = msgpack::type;
      // pack to sbuffer
      mp::sbuffer buffer;
      {
        mp::packer<mp::sbuffer> packer(buffer);
        packer.pack_array(2);
        packer.pack(method);
        packer.pack_array(sizeof...(args));
        (packer.pack(std::forward<Ts>(args)), ...);
      }

      // send and wait for response (blocking)
      zmq::message_t msg(buffer.size());
      m_socket.send(zmq::const_buffer(buffer.data(), buffer.size()));
      auto recv_res = m_socket.recv(msg, zmq::recv_flags::none);
      if (!recv_res)
        throw ::tc::socket_error("zmq socket recv() failed or timed out");

      // unpack msgpack object
      {
        auto root_obj = mp::unpack((const char*) msg.data(), msg.size());

        // ensure a 2-element array
        if (root_obj->type != mpt::object_type::ARRAY)
          throw protocol_error(
            "Expected 2-element array (data type was not array)");
        auto& root_arr = root_obj->via.array;
        if (root_arr.size != 2)
          throw protocol_error(
            "Expected 2-element array (array was not 2 elements long)");

        // check first object's type
        switch (root_arr.ptr[0].type) {
          case mpt::object_type::NIL: {
            // success, return result (or none if void)
            try {
              if constexpr (std::is_void_v<R>) {
                return;
              }
              else {
                return root_arr.ptr[1].as<R>();
              }
            }
            catch (const mp::type_error& err) {
              std::throw_with_nested(
                protocol_error("Receiver returned invalid type for request"));
            }
          } break;
          case mpt::object_type::STR: {
            // error, throw
            try {
              auto err_type = root_arr.ptr[0].as<std::string_view>();
              auto err_msg  = root_arr.ptr[1].as<std::string_view>();
              throw tc::protocol_error(
                fmt::format("Receiver threw {}: {}", err_type, err_msg));
            }
            catch (const mp::type_error& err) {
              // tc::protocol_error won't trigger this
              std::throw_with_nested(protocol_error(
                "Receiver did not return a string for error message"));
            }
          } break;
          default: {
            throw tc::protocol_error("Expected either nil or str at obj[0]");
          } break;
        }
      }
    }

    std::chrono::milliseconds ping() {
      using namespace std::literals;
      namespace chr = std::chrono;
      // corresponds to a single msgpack null value
      static const char data[1] = {static_cast<char>(0xC0)};
      // send null (ping message)
      auto start_time = chr::high_resolution_clock::now();
      zmq::message_t msg {};
      m_socket.send(zmq::const_buffer(data, sizeof(data)));
      // receive message
      auto recv_res = m_socket.recv(msg, zmq::recv_flags::none);
      if (!recv_res)
        throw ::tc::socket_error("zmq socket recv() failed or timed out");
      //
      return chr::duration_cast<chr::milliseconds>(
        chr::high_resolution_clock::now() - start_time);
    }

  private:
    zmq::socket_t m_socket;
  };

}  // namespace tc

#endif