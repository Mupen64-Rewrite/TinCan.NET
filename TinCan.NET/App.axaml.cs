using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using TinCan.NET.Models;
using TinCan.NET.ViewModels;
using TinCan.NET.Views;
using Control = TinCan.NET.Helpers.Control;

namespace TinCan.NET;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        
        if (ApplicationLifetime is not IControlledApplicationLifetime ltControlled)
        {
            Environment.Exit(69);
            return;
        }
        ltControlled.Exit += OnExit;
        switch (ltControlled)
        {
            case IClassicDesktopStyleApplicationLifetime ltDesktop:
            {

                if (ltDesktop.Args is {Length: >= 1})
                {
                    _cancelSource = new CancellationTokenSource();
                    _postbox = new Postbox(ltDesktop.Args[0]);
                    _postboxLoop = new Thread(PostboxLoop);
                    Console.WriteLine("GUI postbox started");
                    InitPostboxHandlers();
                    ltDesktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    
                    // notify C++ side that we are ready
                    _postboxLoop.Start(_cancelSource.Token);
                    _postbox.Enqueue("Ready");
                    
                    // Setup extra args
                    if (ltDesktop.Args.Length >= 3)
                    {
                        var winHandle = nint.Parse(ltDesktop.Args[1], NumberStyles.HexNumber);
                        if (winHandle != 0)
                        {
                            _mainWindow = winHandle;
                            WindowUtils.SetWindowSystemID(ltDesktop.Args[2]);
                        }
                    }
                    
                }
                else
                {
                    ltDesktop.MainWindow = new MainWindow()
                    {
                        DataContext = new MainWindowViewModel()
                    };
                }
                
                break;
            }
            default:
            {
                ltControlled.Shutdown(69);
                break;
            }
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _cancelSource?.Cancel();
        e.ApplicationExitCode = 0;
    }
    
    private void PostboxLoop(object? stopToken)
    {
        CancellationToken ct = (CancellationToken) (stopToken ?? throw new ArgumentNullException(nameof(stopToken)));
        while (true)
        {
            _postbox!.EventLoop(in ct);
            if (ct.IsCancellationRequested)
                return;
        }
    }

    private void InitPostboxHandlers()
    {
        _postbox!.Listen("Shutdown", () =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lt = Application.Current?.ApplicationLifetime;
                if (lt is IControlledApplicationLifetime ltControlled)
                {
                    ltControlled.Shutdown();
                }
                else
                {
                    Environment.Exit(0);
                }
            });
        });
        _postbox!.Listen("RequestUpdateControls", () =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lt = Application.Current?.ApplicationLifetime;
                if (lt is IClassicDesktopStyleApplicationLifetime ltDesktop)
                {
                    var vm = new MainWindowViewModel();
                    var win = new MainWindow
                    {
                        DataContext = vm,
                        CanResize = false
                    };
                    win.Hide();
                    ltDesktop.MainWindow = win;
                }
            });
            
            _postbox!.Enqueue("UpdateControls", 0, true, 0, Control.PluginType.None, Control.ControllerType.Standard);
        });
        _postbox!.Listen("RequestUpdateInputs", (int index) =>
        {
            // no support for ports 1, 2, 3
            if (index != 0)
            {
                _postbox!.Enqueue("UpdateInputs", index, 0);
                return;
            }
            // query UI for port value
            var value = Dispatcher.UIThread.Invoke(() =>
            {
                var lt = Application.Current?.ApplicationLifetime;
                if (lt is not IClassicDesktopStyleApplicationLifetime ltDesktop)
                    return (uint) 0;

                var win = (MainWindow?) ltDesktop.MainWindow;
                if (win == null)
                    return (uint) 0;

                win.ViewModel.OnUpdate();
                
                return win.ViewModel.Value;
            });
            _postbox!.Enqueue("UpdateInputs", 0, value);
        });
        _postbox!.Listen("ShowWindows", () =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lt = Application.Current?.ApplicationLifetime;
                if (lt is IClassicDesktopStyleApplicationLifetime ltDesktop)
                {
                    ltDesktop.MainWindow?.Show();
                }
            });
        });
        _postbox!.Listen("HideWindows", () =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lt = Application.Current?.ApplicationLifetime;
                if (lt is IClassicDesktopStyleApplicationLifetime ltDesktop)
                {
                    ltDesktop.MainWindow?.Hide();
                }
            });
        });
        
    }

    public async Task<bool> PingHost()
    {
        // TODO: Refactor this to MVVM
        if (_postbox == null)
            return false;
        using var handle = _postbox.Wait("ClientPingReply");
        _postbox.Enqueue("ClientPing");
        try
        {
            await handle.Completion.WaitAsync(TimeSpan.FromSeconds(3));
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public void ActivateMainWindow()
    {
        // TODO: Refactor this to MVVM
        if (_mainWindow is not { } mainWindow)
            return;
        
        WindowUtils.ActivateWindow(mainWindow);
    }

    private Postbox? _postbox;
    private CancellationTokenSource? _cancelSource;
    private Thread? _postboxLoop;

    private nint? _mainWindow;
}