using Avalonia;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using TinCan.NET.Models;

namespace TinCan.NET;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        SetupConsole();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    
    
    [DllImport("kernel32.dll", EntryPoint = "AttachConsole", SetLastError = true)]
    [return : MarshalAs(UnmanagedType.Bool)]
    private static extern bool Win32_AttachConsole(uint dwProcessId);

    private const uint Win32_AttachParentProcess = uint.MaxValue;

    private static void SetupConsole()
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (Win32_AttachConsole(Win32_AttachParentProcess))
            return;
        
        int err = Marshal.GetLastPInvokeError();
        if (err != 6)
            throw new Win32Exception(err);
    }
}