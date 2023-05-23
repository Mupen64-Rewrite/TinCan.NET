using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;

namespace TinCan.NET.Models;

public class Postbox
{
    public Postbox(string uri)
    {
        _sock = new PairSocket();
        _sock.Connect(uri);

        _toSend = new ConcurrentQueue<Msg>();
        _recvHandlers = new Dictionary<string, Action<dynamic>>();
    }

    public void EventLoop(in CancellationToken stopFlag)
    {
        bool didAnything = false;

        try
        {
            var recvResult = _sock.TryReceiveFrameBytes(TimeSpan.Zero, out var data);
            if (recvResult)
            {
                didAnything = true;
                Console.WriteLine("RECEIVED");
                // unpack data
                var unpacked = MessagePackSerializer.Deserialize<dynamic>(data);
                // ensure correct formatting
                if (unpacked.GetType() != typeof(object[]))
                    throw new ApplicationException("Bad format (!Array.isArray(root))");
                if (unpacked.Length != 2)
                    throw new ApplicationException("Bad format (root.length != 2)");
            
                // find destination and invoke it
                if (_recvHandlers.ContainsKey(unpacked[0]))
                {
                    _recvHandlers[unpacked[0]](unpacked[1]);
                }
                else
                {
                    FallbackHandler(unpacked[0], unpacked[1]);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        try
        {
            if (!_toSend.IsEmpty)
            {
                didAnything = true;
                _toSend.TryDequeue(out var sendMsg);
                Console.WriteLine("Postbox received message, sending...");
                _sock.TrySend(ref sendMsg, TimeSpan.Zero, false);
                _sock.TrySendFrame(new byte[0]);
                Console.WriteLine("Sent!");
            }
        }
        catch (Exception)
        {
            // ignored
        }

        if (!didAnything && !stopFlag.IsCancellationRequested)
        {
            Thread.Sleep(10);
        }
    }

    public void Enqueue(string @event, params object[] args)
    {
        var data = new object[] { @event, args };
        var buffer = new ArrayBufferWriter<byte>();
        var msg = new Msg();
        MessagePackSerializer.Serialize(buffer, data);
        msg.InitPool(buffer.GetSpan().Length);
        buffer.WrittenMemory.TryCopyTo(msg.SliceAsMemory());
        _toSend.Enqueue(msg);
    }

    public void Listen(string @event, Action<dynamic> listener)
    {
        _recvHandlers[@event] = listener;
    }

    public void Unlisten(string @event)
    {
        _recvHandlers.Remove(@event);
    }

    internal PairSocket Socket => _sock;

    public event Action<string, dynamic> FallbackHandler; 

    private PairSocket _sock;
    private ConcurrentQueue<Msg> _toSend;
    private Dictionary<string, Action<dynamic>> _recvHandlers;
}