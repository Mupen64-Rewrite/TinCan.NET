#ifndef TINCAN_MP_CHECK_HPP
#define TINCAN_MP_CHECK_HPP

#include <array>
#include <type_traits>
#include <utility>
#include "api.hpp"
#include <msgpack.hpp>
namespace tc {
  namespace details {
    template <class T>
    inline constexpr bool msgpack_can_pack =
      requires(T x, msgpack::packer<msgpack::sbuffer>& packer) {
        x.msgpack_pack(packer);
      } || requires(T x, msgpack::packer<msgpack::sbuffer>& packer) {
             msgpack::adaptor::pack<T>()(packer, x);
           };
    template <class T>
    inline constexpr bool msgpack_can_convert =
      requires(T x, const msgpack::object& obj) { x.msgpack_unpack(obj); } ||
      requires(T x, const msgpack::object& obj) {
        msgpack::adaptor::convert<T>()(obj, x);
      };
  }  // namespace details

  template <class T>
  struct is_msgpack_serializable
    : std::bool_constant<
        details::msgpack_can_pack<T> && details::msgpack_can_convert<T>> {};

  template <class T, size_t N>
  struct is_msgpack_serializable<std::array<T, N>>
    : is_msgpack_serializable<T> {};

  template <class T, class U>
  struct is_msgpack_serializable<std::pair<T, U>>
    : std::conjunction<is_msgpack_serializable<T>, is_msgpack_serializable<U>> {
  };

  template <class T>
  inline constexpr bool is_msgpack_serializable_v =
    is_msgpack_serializable<T>::value;
}  // namespace tc

#endif