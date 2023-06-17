using Avalonia;
using System;
using System.Linq;
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
#if false
        TestSocketClient.Run();
        #else
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        #endif
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}