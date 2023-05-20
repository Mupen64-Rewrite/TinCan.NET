#ifndef TINCAN_API_HPP
#define TINCAN_API_HPP

#include <utility>
#include "mupen64plus/m64p_plugin.h"
#include <msgpack.hpp>

namespace tc::api {
  using control_status = std::pair<CONTROL, BUTTONS>;
}

namespace msgpack::adaptor {
  template <>
  class convert<CONTROL> {
    const msgpack::object& operator()(
      const msgpack::object& obj, CONTROL& res) {
      msgpack::type::make_define_array(
        res.Present, res.RawData, res.Plugin, res.Type)
        .msgpack_unpack(obj);
      return obj;
    }
  };
  template <>
  class pack<CONTROL> {
    template <class S>
    msgpack::packer<S>& operator()(msgpack::packer<S>& packer, const CONTROL& res) {
      msgpack::type::make_define_array(
        res.Present, res.RawData, res.Plugin, res.Type)
        .msgpack_pack(packer);
      return packer;
    }
  };
  
  template <>
  class convert<BUTTONS> {
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
  class pack<BUTTONS> {
    template <class S>
    msgpack::packer<S>& operator()(msgpack::packer<S>& packer, const BUTTONS& obj) {
      packer.pack_bin(4);
      packer.pack_bin_body(reinterpret_cast<const char*>(&obj.Value), sizeof(obj));
      return packer;
    }
  };
}  // namespace msgpack::adaptor

#endif