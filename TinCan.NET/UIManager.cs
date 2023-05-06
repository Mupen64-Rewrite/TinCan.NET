using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Silk.NET.SDL;
using TinCan.NET.Helpers;
using TinCan.NET.ViewModels;
using Thread = System.Threading.Thread;

namespace TinCan.NET;

public sealed class UIManager
{
    public void StartUIThread()
    {
        if (_uiThread != null)
            return;
        _uiThread = new Thread(() => { });
        _uiThread.Start();
    }

    public void StopUIThread()
    {
        if (_uiThread == null)
            return;
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
                return;
            lifetime.Shutdown();
        });
        if (_uiThread.IsAlive)
            _uiThread.Join();
    }

    public void ForwardSDLKeyDown(Keymod keymod, Scancode scancode)
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
                RawKeyEventType.KeyDown, SDLHelpers.ToAvaloniaKey(scancode),
                SDLHelpers.ToAvaloniaInputModifiers(keymod)));
        });
    }
    public void ForwardSDLKeyUp(Keymod keymod, Scancode scancode)
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
                RawKeyEventType.KeyUp, SDLHelpers.ToAvaloniaKey(scancode),
                SDLHelpers.ToAvaloniaInputModifiers(keymod)));
        });
    }
    

    public Buttons QueryInputState()
    {
        return new Buttons { Value = 0 };
    }

    private Thread? _uiThread;
    private WeakReference<MainWindowViewModel>? _mainViewModel;
}