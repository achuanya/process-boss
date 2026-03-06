using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ProcessBoss.Models;

namespace ProcessBoss.Services
{
    public class ProcessOptimizer
    {
        // P/Invoke for Efficiency Mode
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessInformation(IntPtr hProcess, int ProcessInformationClass, ref PROCESS_POWER_THROTTLING_STATE ProcessInformation, uint ProcessInformationSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessPriorityBoost(IntPtr hProcess, bool DisablePriorityBoost);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, IntPtr ProcessInformation, int ProcessInformationLength);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern int D3DKMTSetProcessSchedulingPriorityClass(IntPtr hProcess, int Priority);

        private const int ProcessPowerThrottling = 4;
        private const int ProcessIoPriorityInfoClass = 33;
        private const int ProcessMemoryPriorityInfoClass = 39;

        private const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
        private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 1;
        private const uint PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION = 2; 

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_POWER_THROTTLING_STATE
        {
            public uint Version;
            public uint ControlMask;
            public uint StateMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_PRIORITY_INFORMATION
        {
            public uint MemoryPriority;
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
                    process.Kill(true); 
                    return;
                }

                // 1. Efficiency Mode (Eco Mode)
                if (rule.EnableEfficiencyMode)
                {
                    SetEfficiencyMode(process.Handle, true);
                }
                else
                {
                    // Restore to false if unchecked? 
                    // Usually if rule is updated to false, we want to disable it.
                    // If it was already false, no harm.
                    SetEfficiencyMode(process.Handle, false);
                }

                // 2. Priority
                try
                {
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
                            case ProcessPriority.Idle:
                                process.PriorityClass = ProcessPriorityClass.Idle; 
                                break;
                        }
                    }
                    else
                    {
                        // User requested "Unchanged" (Default), so restore to Normal
                        process.PriorityClass = ProcessPriorityClass.Normal;
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"Failed to set CPU Priority: {ex.Message}"); }

                // 3. Affinity
                try
                {
                    if (rule.CpuAffinityMask != 0)
                    {
                        process.ProcessorAffinity = (IntPtr)rule.CpuAffinityMask;
                    }
                    else
                    {
                        // Restore to all processors
                        long allCores = (1L << Environment.ProcessorCount) - 1;
                        process.ProcessorAffinity = (IntPtr)allCores;
                    }
                }
                catch (Exception) { /* Ignore if invalid mask for this system */ }

                // 6. Dynamic Thread Priority Boost
                if (rule.EnableDynamicThreadPriorityBoost.HasValue)
                {
                    try
                    {
                        // API expects 'DisablePriorityBoost', so true means Disabled.
                        bool disable = !rule.EnableDynamicThreadPriorityBoost.Value;
                        SetProcessPriorityBoost(process.Handle, disable);
                    }
                    catch (Exception ex) { Debug.WriteLine($"Failed to set Priority Boost: {ex.Message}"); }
                }
                else
                {
                    // Restore to Default (Enabled)
                    try
                    {
                        SetProcessPriorityBoost(process.Handle, false); // Disable = false => Enabled
                    }
                    catch { }
                }

                // 7. I/O Priority
                if (rule.IoPriority != ProcessIoPriority.Unchanged)
                {
                    SetIoPriority(process.Handle, rule.IoPriority);
                }
                else
                {
                    // Restore to Normal
                    SetIoPriority(process.Handle, ProcessIoPriority.Normal);
                }

                // 8. Memory Priority
                if (rule.MemoryPriority != ProcessMemoryPriority.Unchanged)
                {
                    SetMemoryPriority(process.Handle, rule.MemoryPriority);
                }
                else
                {
                    // Restore to Normal
                    SetMemoryPriority(process.Handle, ProcessMemoryPriority.Normal);
                }

                // 9. GPU Priority
                if (rule.GpuPriority != ProcessGpuPriority.Unchanged)
                {
                    SetGpuPriority(process.Handle, rule.GpuPriority);
                }
                else
                {
                    // Restore to Normal
                    SetGpuPriority(process.Handle, ProcessGpuPriority.Normal);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply rule for {process.ProcessName}: {ex.Message}");
            }
        }

        public void RestoreDefaults(Process process)
        {
            try
            {
                if (process.HasExited) return;

                // 1. Efficiency Mode -> Off
                SetEfficiencyMode(process.Handle, false);

                // 2. Priority -> Normal
                try { process.PriorityClass = ProcessPriorityClass.Normal; } catch { }

                // 3. Affinity -> All Cores
                try
                {
                    long allCores = (1L << Environment.ProcessorCount) - 1;
                    process.ProcessorAffinity = (IntPtr)allCores;
                }
                catch { }

                // 4. Boost -> Enabled
                try { SetProcessPriorityBoost(process.Handle, false); } catch { }

                // 5. I/O -> Normal
                SetIoPriority(process.Handle, ProcessIoPriority.Normal);

                // 6. Memory -> Normal
                SetMemoryPriority(process.Handle, ProcessMemoryPriority.Normal);

                // 7. GPU -> Normal
                SetGpuPriority(process.Handle, ProcessGpuPriority.Normal);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to restore defaults for {process.ProcessName}: {ex.Message}");
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

        private void SetIoPriority(IntPtr hProcess, ProcessIoPriority priority)
        {
            try
            {
                int ioPriority = (int)priority;
                IntPtr ptr = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(ptr, ioPriority);
                try
                {
                    NtSetInformationProcess(hProcess, ProcessIoPriorityInfoClass, ptr, sizeof(int));
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Failed to set I/O Priority: {ex.Message}"); }
        }

        private void SetMemoryPriority(IntPtr hProcess, ProcessMemoryPriority priority)
        {
            try
            {
                var info = new MEMORY_PRIORITY_INFORMATION { MemoryPriority = (uint)priority };
                int size = Marshal.SizeOf(info);
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(info, ptr, false);
                try
                {
                    NtSetInformationProcess(hProcess, ProcessMemoryPriorityInfoClass, ptr, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Failed to set Memory Priority: {ex.Message}"); }
        }

        private void SetGpuPriority(IntPtr hProcess, ProcessGpuPriority priority)
        {
            try
            {
                D3DKMTSetProcessSchedulingPriorityClass(hProcess, (int)priority);
            }
            catch (Exception ex) { Debug.WriteLine($"Failed to set GPU Priority: {ex.Message}"); }
        }
    }
}
