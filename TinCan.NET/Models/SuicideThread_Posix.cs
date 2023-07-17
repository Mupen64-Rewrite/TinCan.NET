using System.Runtime.InteropServices;

namespace TinCan.NET.Models;

public partial class SuicideThread
{
    [DllImport("libc", EntryPoint = "getppid")]
    private static extern int getppid_Linux();

    private static partial bool Check_Linux()
    {
        return getppid_Linux() == 1;
    }
}