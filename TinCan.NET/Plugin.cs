using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using DNNE;
using Silk.NET.SDL;
using TinCan.NET.Helpers;
using Thread = System.Threading.Thread;

namespace TinCan.NET;

public static unsafe class Plugin
{
    private static readonly UCString PluginName = new("TinCan.NET");
    private const string ConstString = "const char*";

    #region Unmanaged entry points

    [C99DeclCode(@"
#include ""m64p_plugin.h""
typedef void (*debug_callback_t)(void*, int, const char*);")]
    [UnmanagedCallersOnly(EntryPoint = nameof(PluginStartup),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: C99Type("m64p_error")]
    public static MupenError PluginStartup(IntPtr coreHandle, void* debugContext,
        [C99Type("debug_callback_t")] delegate* unmanaged<void*, int, IntPtr, void> debugCallback)
    {
        DebugContext = debugContext;
        DebugCallback = debugCallback;

        return MupenError.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(PluginShutdown),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: C99Type("m64p_error")]
    public static MupenError PluginShutdown()
    {
        return MupenError.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(PluginGetVersion),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: C99Type("m64p_error")]
    public static MupenError PluginGetVersion([C99Type("m64p_plugin_type*")] MupenPluginType* pluginType,
        [C99Type("int*")] int* pluginVersion, [C99Type("int*")] int* apiVersion,
        [C99Type("const char**")] byte** pluginName, [C99Type("int*")] int* capabilities)
    {
        if (pluginType != null)
            *pluginType = MupenPluginType.Input;

        if (pluginVersion != null)
        {
            var ver = Assembly.GetCallingAssembly().GetName().Version;
            if (ver != null)
                *pluginVersion = ((byte) ver.Major << 16) | ((byte) ver.Minor << 8) | ((byte) ver.MinorRevision);
            else
                return MupenError.Internal;
        }

        if (apiVersion != null)
            *apiVersion = 0x020100;

        if (pluginName != null)
            *pluginName = PluginName;

        if (capabilities != null)
            *capabilities = 0;

        return MupenError.Success;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(RomOpen),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: C99Type("int")]
    public static int RomOpen()
    {
        _uiThread = new Thread(() =>
        {
            var builder = AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
            builder.StartWithClassicDesktopLifetime(Array.Empty<string>());
        });
        _uiThread.Start();
        return 1;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(RomClosed),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void RomClosed()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }).Wait();
        
        if (_uiThread?.IsAlive ?? false)
            _uiThread.Join();
        _uiThread = null;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(ControllerCommand),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void ControllerCommand([C99Type("int")] int control, [C99Type("const char*")] byte* command) { }

    [UnmanagedCallersOnly(EntryPoint = nameof(GetKeys),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void GetKeys([C99Type("int")] int control, [C99Type("BUTTONS*")] Buttons* buttons)
    {
        // this is temporary
        buttons->Value = 0x00000000;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(InitiateControllers),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void InitiateControllers([C99Type("CONTROL_INFO")] ControlInfo controlInfo)
    {
        // will need to somehow handle multiple controllers
        ControlInfo = controlInfo.Controls;
        ControlInfo[0].Present = 1;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(ReadController),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void ReadController([C99Type("int")] int control, [C99Type("const char*")] IntPtr command) { }

    [UnmanagedCallersOnly(EntryPoint = nameof(SDL_KeyDown),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void SDL_KeyDown([C99Type("int")] Keymod keymod, [C99Type("int")] Scancode scancode)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var lifetime = Application.Current?.ApplicationLifetime;
            if (lifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var inputMgr = InputManager.Instance!;
            var kbDev = KeyboardDevice.Instance!;
            
            // Inject key input into event loop
            inputMgr.ProcessInput(new RawKeyEventArgs(kbDev, (ulong) DateTime.Now.Ticks, desktop.MainWindow!,
                RawKeyEventType.KeyDown, SDLHelpers.ToAvaloniaKey(scancode), SDLHelpers.ToAvaloniaInputModifiers(keymod)));
        });
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(SDL_KeyUp),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void SDL_KeyUp([C99Type("int")] Keymod keymod, [C99Type("int")] Scancode scancode)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var lifetime = Application.Current?.ApplicationLifetime;
            if (lifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var inputMgr = InputManager.Instance!;
            var kbDev = KeyboardDevice.Instance!;
            
            // Inject key input into event loop
            inputMgr.ProcessInput(new RawKeyEventArgs(kbDev, (ulong) DateTime.Now.Ticks, desktop.MainWindow!,
                RawKeyEventType.KeyUp, SDLHelpers.ToAvaloniaKey(scancode), SDLHelpers.ToAvaloniaInputModifiers(keymod)));
        });
    }

    #endregion

    public static void Log(MupenMsgLevel level, string msg)
    {
        if (DebugContext == null || DebugCallback == null)
            return;
        IntPtr mem = Marshal.StringToHGlobalAnsi(msg);
        try
        {
            DebugCallback(DebugContext, (int) level, mem);
        }
        finally
        {
            Marshal.FreeHGlobal(mem);
        }
    }

    private static void* DebugContext;
    private static delegate* unmanaged<void*, int, IntPtr, void> DebugCallback;

    private static Control* ControlInfo;

    private static Thread? _uiThread;
}