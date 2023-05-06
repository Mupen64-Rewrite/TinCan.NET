using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
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

    #region Unmanaged entry points

    /// <summary>
    /// Performs any initial setup required by the plugin.
    /// </summary>
    /// <param name="coreHandle">A native library handle to the core.</param>
    /// <param name="debugContext">An opaque pointer to pass to <paramref name="debugCallback"/>.</param>
    /// <param name="debugCallback">A function to log data back to the frontend.</param>
    /// <returns>An error code, or <see cref="MupenError.Success"/> otherwise.</returns>
    [C99DeclCode(@"
#include ""m64p_plugin.h""
typedef void (*debug_callback_t)(void*, int, const char*);")]
    [UnmanagedCallersOnly(EntryPoint = nameof(PluginStartup),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: C99Type("m64p_error")]
    public static MupenError PluginStartup(IntPtr coreHandle, void* debugContext,
        [C99Type("debug_callback_t")] delegate* unmanaged[Cdecl]<void*, int, IntPtr, void> debugCallback)
    {
        DebugContext = debugContext;
        DebugCallback = debugCallback;

        _uiManager = new UIManager();

        return MupenError.Success;
    }

    /// <summary>
    /// Performs any final cleanup before unloading the plugin.
    /// </summary>
    /// <returns>An error code, or <see cref="MupenError.Success"/> otherwise.</returns>
    [UnmanagedCallersOnly(EntryPoint = nameof(PluginShutdown),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: C99Type("m64p_error")]
    public static MupenError PluginShutdown()
    {
        _uiManager.StopUIThread();
        
        return MupenError.Success;
    }


    /// <summary>
    /// Requests the plugin to return version information about itself.
    /// </summary>
    /// <param name="pluginType">A pointer to set to the type of plugin.</param>
    /// <param name="pluginVersion">A pointer to set to the plugin version.</param>
    /// <param name="apiVersion">A pointer to set to the API version.</param>
    /// <param name="pluginName">A pointer to set to a static string of the plugin name.</param>
    /// <param name="capabilities">A pointer to set to the capabilities. This only applies to the Mupen64Plus core,
    ///     so it should be set to 0.</param>
    /// <returns></returns>
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


    /// <summary>
    /// Called by Mupen64Plus upon opening the ROM.
    /// </summary>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = nameof(RomOpen),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    [return: C99Type("int")]
    public static int RomOpen()
    {
        _uiManager.StartUIThread();
        return 1;
    }

    /// <summary>
    /// Called by Mupen64Plus upon closing the ROM.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = nameof(RomClosed),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void RomClosed()
    {
        _uiManager.StopUIThread();
    }

    /// <summary>
    /// Called by Mupen64Plus to send miscellaneous commands to the controller (e.g. rumble).
    /// </summary>
    /// <param name="control">The index of the controller</param>
    /// <param name="command">The command (a fixed-size byte array)</param>
    [UnmanagedCallersOnly(EntryPoint = nameof(ControllerCommand),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void ControllerCommand([C99Type("int")] int control, [C99Type("const char*")] byte* command) { }

    /// <summary>
    /// Called by Mupen64Plus to request the current input state for a controller.
    /// </summary>
    /// <param name="control">The index of the controller.</param>
    /// <param name="buttons">A pointer to a <see cref="Buttons"/> struct to write the state to.</param>
    [UnmanagedCallersOnly(EntryPoint = nameof(GetKeys),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void GetKeys([C99Type("int")] int control, [C99Type("BUTTONS*")] Buttons* buttons)
    {
        // this is temporary
        buttons->Value = 0;
    }

    /// <summary>
    /// Called by Mupen64Plus to send a shared struct containing controller inputs.
    /// </summary>
    /// <param name="controlInfo"></param>
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

    /// <summary>
    /// Called by Mupen64Plus upon receiving a "key down" event from the frontend.
    /// </summary>
    /// <param name="keymod">The modifier keys applied</param>
    /// <param name="scancode">The key released</param>
    [UnmanagedCallersOnly(EntryPoint = nameof(SDL_KeyDown),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void SDL_KeyDown([C99Type("int")] Keymod keymod, [C99Type("int")] Scancode scancode)
    {
        _uiManager.ForwardSDLKeyDown(keymod, scancode);
    }

    /// <summary>
    /// Called by Mupen64Plus upon receiving a "key up" event from the frontend.
    /// </summary>
    /// <param name="keymod">The modifier keys applied</param>
    /// <param name="scancode">The key released</param>
    [UnmanagedCallersOnly(EntryPoint = nameof(SDL_KeyUp),
        CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static void SDL_KeyUp([C99Type("int")] Keymod keymod, [C99Type("int")] Scancode scancode)
    {
        _uiManager.ForwardSDLKeyUp(keymod, scancode);
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
    private static delegate* unmanaged[Cdecl]<void*, int, IntPtr, void> DebugCallback;

    private static Control* ControlInfo;

    private static Thread? _uiThread;
    private static UIManager _uiManager;
}