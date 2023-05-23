using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        
        // ask for connection path
        Console.Write("Input socket URI: ");
        string sockPath = Console.ReadLine()!;

        _postbox = new Postbox(sockPath);
        _postbox.FallbackHandler += (name, param) =>
        {
            Console.WriteLine($"Received request {name}");
        };
        _stopSource = new CancellationTokenSource();
        
        _postboxLoop = new Thread(PostboxLoop);
        _postboxLoop.Start(_stopSource.Token);
        
        desktop.Exit += DesktopOnExit;
        
        desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }

    private void PostboxLoop(object? stopToken)
    {
        CancellationToken ct = (CancellationToken) (stopToken ?? throw new ArgumentNullException(nameof(stopToken)));
        while (true)
        {
            _postbox.EventLoop(in ct);
            if (ct.IsCancellationRequested)
                return;
        }
    }

    private void DesktopOnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _stopSource.Cancel();
        e.ApplicationExitCode = 0;
    }


    private Postbox _postbox;
    private CancellationTokenSource _stopSource;
    private Thread? _postboxLoop;
}