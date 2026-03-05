using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SKM.Models;

namespace SKM.Services
{
    public class ProcessOptimizer
    {
        // P/Invoke for Efficiency Mode
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessInformation(IntPtr hProcess, int ProcessInformationClass, ref PROCESS_POWER_THROTTLING_STATE ProcessInformation, uint ProcessInformationSize);

        private const int ProcessPowerThrottling = 4;
        private const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
        private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 1;
        private const uint PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION = 2; // Reserved, but good to know

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_POWER_THROTTLING_STATE
        {
            public uint Version;
            public uint ControlMask;
            public uint StateMask;
        }

        public void ApplyRule(Process process, ProcessRule rule)
        {
            try
            {
                if (process.HasExited) return;

                // 4. Kill Process
                if (rule.KillOnStart)
                {
                    process.Kill();
                    return;
                }

                // 5. Kill Process Tree
                if (rule.KillTreeOnStart)
                {
                    process.Kill(true); // .NET 8 supports Kill(true) for tree
                    return;
                }

                // 1. Efficiency Mode (Eco Mode)
                if (rule.EnableEfficiencyMode)
                {
                    SetEfficiencyMode(process.Handle, true);
                }

                // 2. Priority
                if (rule.Priority != ProcessPriority.Unchanged)
                {
                    switch (rule.Priority)
                    {
                        case ProcessPriority.RealTime:
                            process.PriorityClass = ProcessPriorityClass.RealTime;
                            break;
                        case ProcessPriority.High:
                            process.PriorityClass = ProcessPriorityClass.High;
                            break;
                        case ProcessPriority.AboveNormal:
                            process.PriorityClass = ProcessPriorityClass.AboveNormal;
                            break;
                        case ProcessPriority.Normal:
                            process.PriorityClass = ProcessPriorityClass.Normal;
                            break;
                        case ProcessPriority.BelowNormal:
                            process.PriorityClass = ProcessPriorityClass.BelowNormal;
                            break;
                        case ProcessPriority.Low:
                            process.PriorityClass = ProcessPriorityClass.Idle; // "Low" maps to Idle in .NET enum usually, or strictly Idle
                            break;
                    }
                }

                // 3. Affinity
                if (rule.CpuAffinityMask != 0)
                {
                    // Only apply if the mask is valid for the current system (simple check)
                    // In a real app, we might want to mask it against Environment.ProcessorCount
                    try
                    {
                        process.ProcessorAffinity = (IntPtr)rule.CpuAffinityMask;
                    }
                    catch (Exception) { /* Ignore if invalid mask for this system */ }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply rule for {process.ProcessName}: {ex.Message}");
            }
        }

        private void SetEfficiencyMode(IntPtr hProcess, bool enable)
        {
            var throttlingState = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                StateMask = enable ? PROCESS_POWER_THROTTLING_EXECUTION_SPEED : 0
            };

            uint size = (uint)Marshal.SizeOf<PROCESS_POWER_THROTTLING_STATE>();
            SetProcessInformation(hProcess, ProcessPowerThrottling, ref throttlingState, size);
        }
    }
}
