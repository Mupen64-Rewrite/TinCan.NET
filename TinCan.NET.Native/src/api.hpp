#ifndef TINCAN_API_HPP
#define TINCAN_API_HPP

#include <utility>
#include "mupen64plus/m64p_plugin.h"
#include <msgpack.hpp>
#include <msgpack/v3/adaptor/adaptor_base.hpp>

namespace tc::api {
  using control_status = std::pair<CONTROL, BUTTONS>;
}

namespace msgpack {
  MSGPACK_API_VERSION_NAMESPACE(MSGPACK_DEFAULT_API_NS) {
    namespace adaptor {
      template <>
      struct convert<CONTROL, void> {
        const msgpack::object& operator()(
          const msgpack::object& obj, CONTROL& res) {
          msgpack::type::make_define_array(
            res.Present, res.RawData, res.Plugin, res.Type)
            .msgpack_unpack(obj);
          return obj;
        }
      };
      template <>
      struct pack<CONTROL, void> {
        template <class S>
        msgpack::packer<S>& operator()(
          msgpack::packer<S>& packer, const CONTROL& res) {
          msgpack::type::make_define_array(
            res.Present, res.RawData, res.Plugin, res.Type)
            .msgpack_pack(packer);
          return packer;
        }
      };

      template <>
      struct convert<BUTTONS, void> {
        const msgpack::object& operator()(
          const msgpack::object& obj, BUTTONS& res) {
          if (obj.type != msgpack::type::object_type::BIN)
            throw msgpack::type_error();
          if (obj.via.bin.size != 4)
            throw msgpack::type_error();

          memcpy(&res, obj.via.bin.ptr, 4);
          return obj;
        }
      };
      template <>
      struct pack<BUTTONS, void> {
        template <class S>
        msgpack::packer<S>& operator()(
          msgpack::packer<S>& packer, const BUTTONS& obj) {
          packer.pack_bin(sizeof(BUTTONS));
          packer.pack_bin_body(
            reinterpret_cast<const char*>(&obj.Value), sizeof(BUTTONS));
          return packer;
        }
      };
    }
  }

  
}  // namespace msgpack::adaptor

#endif