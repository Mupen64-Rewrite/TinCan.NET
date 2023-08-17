using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TinCan.NET.Models;

public static class WindowUtils
{
    public enum WindowSystemID
    {
        None = -1,
        Windows = 0,
        Cocoa,
        X11,
        Wayland
    }

    private static WindowSystemID _windowSystem = WindowSystemID.None;
    public static void SetWindowSystemID(string str)
    {
        if (!Enum.TryParse(str, true, out _windowSystem))
            throw new ArgumentException("Invalid window system name!");
    }
    
    
    public static void ActivateWindow(nint handle)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(5))
            ActivateWindowWin32(handle);
        else if (OperatingSystem.IsLinux())
        {
            switch (_windowSystem)
            {
                case WindowSystemID.X11: 
                    ActivateWindowX11(handle);
                    break;
                case WindowSystemID.Wayland:
                    ActivateWindowWayland(handle);
                    break;
            }
        }
        else if (OperatingSystem.IsMacOSVersionAtLeast(13))
        {
            switch (_windowSystem)
            {
                case WindowSystemID.Cocoa:
                    ActivateWindowCocoa(handle);
                    break;
                case WindowSystemID.X11:
                    ActivateWindowX11(handle);
                    break;
            }
        }
    }

    
    [SupportedOSPlatform("windows5.0")]
    private static void ActivateWindowWin32(nint handle)
    {
        WinAPI.SetForegroundWindow((HWND) handle);
    }

    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos13.0")]
    private static void ActivateWindowX11(nint handle)
    {
        
    }
    
    [SupportedOSPlatform("linux")]
    private static void ActivateWindowWayland(nint handle)
    {
        throw new NotImplementedException();
    }

    [SupportedOSPlatform("macos13.0")]
    private static void ActivateWindowCocoa(nint handle)
    {
        throw new NotSupportedException("We don't support MacOS. Submit a PR if you'd like to implement this.");
    }
}