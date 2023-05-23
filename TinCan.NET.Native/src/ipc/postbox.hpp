#ifndef TC_IPC_POSTBOX_HPP
#define TC_IPC_POSTBOX_HPP

#include "../tl/function_ref.hpp"
#include "../util/string_hash.hpp"
#include "safe_queue.hpp"

#include <iostream>
#include <stdexcept>
#include <stop_token>
#include <thread>
#include <unordered_map>

#include <fmt/core.h>
#include <msgpack.hpp>
#include <msgpack/v3/object_decl.hpp>
#include <msgpack/v3/sbuffer_decl.hpp>
#include <msgpack/v3/unpack_decl.hpp>
#include <zmq.hpp>

namespace tc {
  inline zmq::context_t& zmq_context() {
    static zmq::context_t ctx;
    return ctx;
  }

  class postbox {
  public:
    using listener_type = tl::function_ref<void(const msgpack::object&)>;

    postbox(const char* uri) : m_sock(zmq_context(), zmq::socket_type::pair) {
      m_sock.set(zmq::sockopt::sndtimeo, 50);
      m_sock.set(zmq::sockopt::rcvtimeo, 50);
      m_sock.bind(uri);
    }

    // Performs 1 iteration of the postbox's event loop.
    void event_loop(const std::stop_token& stop) {
      using namespace std::literals;

      bool did_anything = false;
      zmq::message_t msg {};

      try {
        if (auto res = m_sock.recv(msg, zmq::recv_flags::dontwait);
            res.has_value()) {
          did_anything = true;
          // unpack data
          msgpack::object_handle data;
          msgpack::unpack(data, (const char*) msg.data(), msg.size());

          // ensure correct formatting
          if (data->type != msgpack::type::object_type::ARRAY)
            throw std::runtime_error("Bad format (!Array.isArray(root))");
          if (data->via.array.size != 2)
            throw std::runtime_error("Bad format (root.length != 2)");

          // find destination and invoke it (I may add a fallback handler as
          // well)
          auto dest = data->via.array.ptr[0].as<std::string_view>();
          if (auto it = m_recv_handlers.find(dest);
              it != m_recv_handlers.end()) {
            it->second(data->via.array.ptr[1]);
          }
        }
      }
      catch (...) {
        // do nothing for now
      }
      try {
        if (!m_to_send.empty()) {
          did_anything = true;
          
          // Message was serialized before enqueuing, just send it
          zmq::message_t to_send;
          m_to_send.pop_return(to_send);
          std::cout << "Postbox received message, sending...\n";
          auto res = m_sock.send(to_send, zmq::send_flags::none);
          if (res.has_value()) {
            std::cout << "Sent!\n";
          }
          else {
            std::cout << "Timed out!\n";
          }
        }
      }
      catch (...) {
        // do nothing for now
      }

      if (!did_anything && !stop.stop_requested()) {
        std::this_thread::sleep_for(10ms);
      }
    }

    // Enqueues an event to be sent to the other side.
    // This is thread-safe.
    template <class... Args>
    void enqueue(std::string_view event, Args&&... args) {
      msgpack::sbuffer buffer;
      msgpack::packer packer {buffer};

      packer.pack_array(2);
      packer.pack(event);
      packer.pack_array(sizeof...(Args));
      ((packer.pack(std::forward<Args>(args))), ...);

      m_to_send.emplace((void*) buffer.data(), buffer.size());
    }

    // Sets the listener for a particular event. Overrides any previously set
    // listeners. Do not use while the event loop is active.
    void listen(std::string_view event, const listener_type& listener) {
      m_recv_handlers.emplace(event, listener);
    }

    // Unset the listener for a particular event if it is already set. Do not
    // use while the event loop is active.
    void unlisten(std::string_view event) {
      if (auto it = m_recv_handlers.find(event); it != m_recv_handlers.end()) {
        m_recv_handlers.erase(it);
      }
    }

  private:
    zmq::socket_t m_sock;
    tc::safe_queue<zmq::message_t> m_to_send;
    tc::string_map<tl::function_ref<void(const msgpack::object&)>>
      m_recv_handlers;
  };
}  // namespace tc

#endif