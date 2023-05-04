using System;
using System.Runtime.InteropServices;

namespace TinCan.NET;

/// <summary>
/// Wrapper for an immutable, unmanaged null-terminated string.
/// </summary>
public unsafe class UCString
{
    public UCString(string value)
    {
        _ptr = Marshal.StringToHGlobalAnsi(value);
    }

    ~UCString()
    {
        Marshal.FreeHGlobal(_ptr);
    }

    public static implicit operator byte*(UCString ucs) => (byte*) ucs._ptr;
    public static implicit operator IntPtr(UCString ucs) => ucs._ptr;
    
    private IntPtr _ptr;
}