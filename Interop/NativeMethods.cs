using System;
using System.Runtime.InteropServices;

namespace ProcessBoss.Interop
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetProcessInformation(IntPtr hProcess, int ProcessInformationClass, ref PROCESS_POWER_THROTTLING_STATE ProcessInformation, uint ProcessInformationSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetProcessPriorityBoost(IntPtr hProcess, bool DisablePriorityBoost);

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern int NtSetInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, IntPtr ProcessInformation, int ProcessInformationLength);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern int D3DKMTSetProcessSchedulingPriorityClass(IntPtr hProcess, int Priority);

        internal const int ProcessPowerThrottling = 4;
        internal const int ProcessIoPriorityInfoClass = 33;
        internal const int ProcessMemoryPriorityInfoClass = 39;

        internal const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
        internal const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 1;
        internal const uint PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION = 2; 

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_POWER_THROTTLING_STATE
        {
            public uint Version;
            public uint ControlMask;
            public uint StateMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORY_PRIORITY_INFORMATION
        {
            public uint MemoryPriority;
        }
    }
}
