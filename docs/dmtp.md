# Discrete Message Transfer Protocol (DMTP)

This specification defines a very basic event-based messaging protocol. It is
intended for internal use within the TinCan.NET project, and as such, is
currently unstable.

DMTP, much like other application-layer protocols, is designed to run on top of
existing network transports such as TCP/IP or Unix sockets.

## Packet structure

All DMTP messages start with a 4-byte signature, followed by a 2-byte message
type. The remainder differs based on specified message type. Any integer values

Message types are represented as 2-byte unsigned integers. The following are
message types defined by the DMTP spec:

```
0x0000: PING
0x0001: MESSAGE
```

### PING structure

The following defines the layout of a PING packet:

```
4 bytes: signature: ASCII 'D', 'M', 'T', 'P'
===========================================================
2 bytes: msg_type (PING)
2 bytes: u16 ping_type: 0x0000 = ping, 0x0001 = pong
4 bytes: u32 ping_id: identifies the ping/pong
===========================================================
```

A DMTP peer that receives a PING packet with `ping_type = 0x0000` is expected
to reply with a corresponding PING packet with `ping_type = 0x0001` and the same
`ping_id` as provided by the sender.

This packet type is provided as a means to test a DMTP connection, and to ensure
the opposing peer is responsive.

### MESSAGE structure

The following defines the layout of a MESSAGE packet:

```
4 bytes: signature: ASCII 'D', 'M', 'T', 'P'
===========================================================
2 bytes: msg_type (MESSAGE)
2 bytes: u16 evt_len: length of event name
evt_len bytes, padded up to the nearest 4: event name
4 bytes: u32 msg_len: length of message
===========================================================
msg_len bytes: arbitrary event data
===========================================================
```

A DMTP peer that receives a MESSAGE packet may handle or discard the event. It
is not required to notify its corresponding peer about what it does with the
data.

This packet type is used for sending and receiving events over the socket.

## Legal

Like the rest of this project, this specification is made available under the
GNU GPL v3. If you choose to specify a derivative protocol, please cite this
document and do not claim ownership.