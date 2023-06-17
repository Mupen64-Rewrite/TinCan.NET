using System;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using TinCan.NET.Models;
using TinCan.NET.ViewModels;
using TinCan.NET.Views;

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

                if (ltDesktop.Args is {Length: 1})
                {
                    _stopSource = new CancellationTokenSource();
                    _postbox = new Postbox(ltDesktop.Args[0]);
                    _postboxLoop = new Thread(PostboxLoop);
                    
                    InitPostboxHandlers();
                    ltDesktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    
                    _postboxLoop.Start(_stopSource.Token);
                }
                else
                {
                    ltDesktop.MainWindow = new MainWindow();
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
        _postbox!.Listen("OpenWindow", () =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lt = Application.Current?.ApplicationLifetime;
                if (lt is IClassicDesktopStyleApplicationLifetime ltDesktop)
                {
                    ltDesktop.MainWindow = new MainWindow();
                }
            });
        });
        _postbox!.Listen("CloseWindow", () =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var lt = Application.Current?.ApplicationLifetime;
                if (lt is IClassicDesktopStyleApplicationLifetime ltDesktop)
                {
                    ltDesktop.MainWindow?.Close();
                    ltDesktop.MainWindow = null;
                }
            });
        });
        
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

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _stopSource?.Cancel();
        e.ApplicationExitCode = 0;
    }


    private Postbox? _postbox;
    private CancellationTokenSource? _stopSource;
    private Thread? _postboxLoop;
}