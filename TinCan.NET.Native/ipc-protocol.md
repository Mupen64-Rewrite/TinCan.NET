# TinCan RPC protocol
The protocol is based on MsgPack and uses ZeroMQ as the transport.
## Requests
Requests are sent in equivalent MsgPack to the following JSON:
```json
[
  "foo", // method
  [420.69, "foobar", true], // parameters
]
```
## Replies
A successful reply looks like this:
```json
[
  null, // error type (null if non-existent)
  504 // result
]
```
An unsuccessful reply might look like this:
```json
[
  ".NET|System.ArgumentNullException", // framework|error type
  "Parameter b is null" // error message
]
```

## Miscellaneous requests
### Ping/Pong
The ping request is simply the msgpack null value. To reply to a ping, send null back.