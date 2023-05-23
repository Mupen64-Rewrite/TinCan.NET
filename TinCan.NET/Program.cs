using Avalonia;
using System;
using System.Threading;
using NetMQ.Monitoring;
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
        Postbox postbox;
        Console.Write("Socket URI: ");
        postbox = new Postbox(Console.ReadLine()!);

        CancellationTokenSource stopSource = new CancellationTokenSource();
        Thread postThread = new Thread((stopToken) =>
        {
            CancellationToken ct = (CancellationToken)(stopToken ?? throw new ArgumentNullException(nameof(stopToken)));
            while (true)
            {
                postbox.EventLoop(in ct);
                if (ct.IsCancellationRequested)
                    return;
            }
        });
        postThread.Start(stopSource.Token);

        while (true)
        {
            string line = Console.ReadLine()!;
            if (line == "exit")
                break;
            var split = line.IndexOf(' ');
            postbox.Enqueue(line[..split], line[split..]);
        }
        
        stopSource.Cancel();
        if (postThread.IsAlive)
            postThread.Join();
        
        return;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}