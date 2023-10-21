using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TinCan.NET.Models.Libraries;

public static unsafe class X11
{
    private const string LibX11 = "libX11.so.6";

    public const ulong CurrentTime = 0UL;

    public const int RevertToNone = 0;
    public const int RevertToPointerRoot = 1;
    public const int RevertToParent = 2;

    public static IntPtr DisplayPtr { get; }

    static X11()
    {
        DisplayPtr = XOpenDisplay(null);
        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            XCloseDisplay(DisplayPtr);
        };
    }

    [DllImport(LibX11)]
    [SupportedOSPlatform("linux")]
    public static extern IntPtr XOpenDisplay(byte* name);

    [DllImport(LibX11)]
    [SupportedOSPlatform("linux")]
    public static extern void XCloseDisplay(IntPtr display);

    [DllImport(LibX11)]
    [SupportedOSPlatform("linux")]
    public static extern void XRaiseWindow(IntPtr display, IntPtr window);

    [DllImport(LibX11)]
    [SupportedOSPlatform("linux")]
    public static extern void XSetInputFocus(IntPtr display, IntPtr window, int revertTo, ulong time);
}