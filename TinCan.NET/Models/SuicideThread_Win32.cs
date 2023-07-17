using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using TinCan.NET.Models.Win32;

namespace TinCan.NET.Models;

public partial class SuicideThread
{
#pragma warning disable CA1416
    private static unsafe Process? ParentProcessWin32()
    {
        var proc = PInvoke.GetCurrentProcess();
        PROCESS_BASIC_INFORMATION basicInfo;
        var status = Win32PInvokeExtras.NtQueryInformationProcess(proc, PROCESSINFOCLASS.ProcessBasicInformation,
            &basicInfo, (uint) Marshal.SizeOf<PROCESS_BASIC_INFORMATION>(), null);
        if (status != (NTSTATUS) 0)
            throw new Win32Exception(status);

        try
        {
            return Process.GetProcessById(basicInfo.InheritedFromUniqueProcessId.ToInt32());
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
    
    private static unsafe partial bool Check_Win32()
    {
        var currProc = Process.GetCurrentProcess();
        var parentProc = ParentProcessWin32();
        
        return parentProc == null || currProc.StartTime > parentProc.StartTime;
    }
#pragma warning restore CA1416
}