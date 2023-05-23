using System;
using System.Buffers;
using System.Collections.Generic;
using MessagePack;
using NetMQ;

namespace TinCan.NET.Models;

public class IPCServer
{
    public IPCServer()
    {
        _handlers = new Dictionary<string, Delegate>();
    }
    
    public void Register(string name, Delegate handler)
    {
        _handlers.Add(name, handler);
    }

    public void Unregister(string name)
    {
        _handlers.Remove(name);
    }

    public IBufferWriter<byte> Process(ReadOnlyMemory<byte> msg)
    {
        var data = MessagePackSerializer.Deserialize<dynamic>(msg);
        object? res = null;
        if (data != null)
        {
            try
            {
                res = new object?[]
                {
                    null,
                    _handlers[(string)data[0]].DynamicInvoke((object?[])data[1])
                };
            }
            catch (Exception e)
            {
                res = new object?[]
                {
                    e.GetType().FullName,
                    e.Message
                };
            }
        }

        var outBuf = new ArrayBufferWriter<byte>();
        MessagePackSerializer.Serialize(outBuf, res);
        return outBuf;
    }

    private Dictionary<string, Delegate> _handlers;
}