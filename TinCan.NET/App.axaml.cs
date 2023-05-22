using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;
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
        desktop.ShutdownRequested += OnAppShutdown;
        desktop.MainWindow = new MainWindow();

        // init the socket if started in socket mode
        if (desktop.Args is { Length: > 2 })
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _server = new IPCServer();
            _server.Register("Shutdown", Server_Shutdown);
            
            _socket = new ResponseSocket($">{desktop.Args[1]}");
            _socketTimer =
                new DispatcherTimer(TimeSpan.FromMilliseconds(50.0), DispatcherPriority.Background, PollSocket);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void PollSocket(object? sender, EventArgs e)
    {
        Console.WriteLine("POLL SOCKET");
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        Msg msg = new();
        _socket.TryReceive(ref msg, TimeSpan.Zero);
        
        var res = _server.Process(msg.SliceAsMemory());
        
        msg.InitPool(res.GetSpan().Length);
        res.GetSpan().CopyTo(msg.Slice());
        _socket.Send(ref msg, false);
    }

    private void Server_Shutdown()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        desktop.Shutdown();
    }



    private void OnAppShutdown(object? sender, ShutdownRequestedEventArgs args)
    {
    }

    private DispatcherTimer _socketTimer = null!;
    private ResponseSocket _socket = null!;
    private IPCServer _server = null!;
}