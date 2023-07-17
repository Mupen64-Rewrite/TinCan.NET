using System;
using System.Diagnostics;
using System.Threading;

namespace TinCan.NET.Models;

/// <summary>
/// Causes TinCan.NET to commit seppuku should it be abandoned.
/// </summary>
public partial class SuicideThread
{
    private static partial bool Check_Win32();
    private static partial bool Check_Linux();

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
    public static void Run()
    {
        while (true)
        {
            if (!Check())
                Process.GetCurrentProcess().Kill();
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}