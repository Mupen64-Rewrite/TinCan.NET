using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace TinCan.NET.Models.Win32;

internal struct PROCESS_BASIC_INFORMATION
{
    internal NTSTATUS ExitStatus;
    internal IntPtr PebBaseAddress;
    internal IntPtr AffinityMask;
    internal IntPtr BasePriority;
    internal IntPtr UniqueProcessId;
    internal IntPtr InheritedFromUniqueProcessId;
}

internal enum PROCESSINFOCLASS
{
    ProcessBasicInformation,
    ProcessQuotaLimits,
    ProcessIoCounters,
    ProcessVmCounters,
    ProcessTimes,
    ProcessBasePriority,
    ProcessRaisePriority,
    ProcessDebugPort,
    ProcessExceptionPort,
    ProcessAccessToken,
    ProcessLdtInformation,
    ProcessLdtSize,
    ProcessDefaultHardErrorMode,
    ProcessIoPortHandlers,
    ProcessPooledUsageAndLimits,
    ProcessWorkingSetWatch,
    ProcessUserModeIOPL,
    ProcessEnableAlignmentFaultFixup,
    ProcessPriorityClass,
    ProcessWx86Information,
    ProcessHandleCount,
    ProcessAffinityMask,
    ProcessPriorityBoost,
    ProcessDeviceMap,
    ProcessSessionInformation,
    ProcessForegroundInformation,
    ProcessWow64Information,
    ProcessImageFileName,
    ProcessLUIDDeviceMapsEnabled,
    ProcessBreakOnTermination,
    ProcessDebugObjectHandle,
    ProcessDebugFlags,
    ProcessHandleTracing,
    ProcessIoPriority,
    ProcessExecuteFlags,
    ProcessTlsInformation,
    ProcessCookie,
    ProcessImageInformation,
    ProcessCycleTime,
    ProcessPagePriority,
    ProcessInstrumentationCallback,
    ProcessThreadStackAllocation,
    ProcessWorkingSetWatchEx,
    ProcessImageFileNameWin32,
    ProcessImageFileMapping,
    ProcessAffinityUpdateMode,
    ProcessMemoryAllocationMode,
    ProcessGroupInformation,
    ProcessTokenVirtualizationEnabled,
    ProcessConsoleHostProcess,
    ProcessWindowInformation,
    MaxProcessInfoClass
}

internal unsafe class Win32PInvokeExtras
{
    [DllImport("ntdll.dll", SetLastError = true)]
    internal static extern NTSTATUS NtQueryInformationProcess(
        HANDLE ProcessHandle, 
        PROCESSINFOCLASS ProcessInformationClass, 
        void* ProcessInformation,
        uint ProcessInformationLength,
        uint* ReturnLength);
}