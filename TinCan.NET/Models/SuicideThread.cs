using System;
using System.Diagnostics;
using System.Threading;

namespace TinCan.NET.Models;

/// <summary>
/// Causes TinCan.NET to commit seppuku should it be abandoned.
/// </summary>
public partial class SuicideThread
{
    private static unsafe partial bool Check_Win32();
    private static unsafe partial bool Check_Linux();

    private static bool Check()
    {
        if (OperatingSystem.IsWindows())
        {
            return Check_Win32();
        }

        if (OperatingSystem.IsLinux())
        {
            return Check_Linux();
        }

        throw new PlatformNotSupportedException("No die for you");
    }
    public static void Run(object? _token)
    {
        CancellationToken token = (CancellationToken) _token!;
        while (!token.IsCancellationRequested)
        {
            if (!Check())
                Process.GetCurrentProcess().Kill();
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}