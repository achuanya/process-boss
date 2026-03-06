using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProcessBoss.Models
{
    public enum ProcessPriority
    {
        RealTime,
        High,
        AboveNormal,
        Normal,
        BelowNormal,
        Idle,
        Unchanged // Default, do not modify
    }

    public enum ProcessIoPriority
    {
        VeryLow = 0,
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4,
        Unchanged = -1
    }

    public enum ProcessMemoryPriority
    {
        Lowest = 0,
        VeryLow = 1,
        Low = 2,
        Medium = 3,
        BelowNormal = 4,
        Normal = 5,
        Unchanged = -1
    }

    public enum ProcessGpuPriority
    {
        Idle = 0,
        BelowNormal = 1,
        Normal = 2,
        AboveNormal = 3,
        High = 4,
        Realtime = 5,
        Unchanged = -1
    }

    public class ProcessRule
    {
        public string ProcessName { get; set; } = string.Empty; // e.g., "notepad.exe"
        public string FullPath { get; set; } = string.Empty; // Optional, for stricter matching
        public string Remarks { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;

        public ProcessPriority Priority { get; set; } = ProcessPriority.Unchanged;
        
        // Bitmask for CPU Affinity. 0 means all cores (or no change).
        // Storing as long to support 64 cores. For >64, we might need a different approach, but long is usually enough for desktop.
        public long CpuAffinityMask { get; set; } = 0; 
        
        public bool EnableEfficiencyMode { get; set; }
        
        // Dynamic Thread Priority Boost
        // null = Unchanged
        // true = Enable
        // false = Disable
        public bool? EnableDynamicThreadPriorityBoost { get; set; }

        public ProcessIoPriority IoPriority { get; set; } = ProcessIoPriority.Unchanged;
        public ProcessMemoryPriority MemoryPriority { get; set; } = ProcessMemoryPriority.Unchanged;
        public ProcessGpuPriority GpuPriority { get; set; } = ProcessGpuPriority.Unchanged;

        public bool KillOnStart { get; set; }
        public bool KillTreeOnStart { get; set; }

        // Localization Helpers
        [JsonIgnore]
        public string PriorityDisplay => GetPriorityName(Priority);
        
        [JsonIgnore]
        public string IoPriorityDisplay => GetIoPriorityName(IoPriority);
        
        [JsonIgnore]
        public string MemoryPriorityDisplay => GetMemoryPriorityName(MemoryPriority);
        
        [JsonIgnore]
        public string GpuPriorityDisplay => GetGpuPriorityName(GpuPriority);

        [JsonIgnore]
        public bool IsPrioritySet => Priority != ProcessPriority.Unchanged;
        [JsonIgnore]
        public bool IsAffinitySet => CpuAffinityMask != 0;
        [JsonIgnore]
        public bool IsDynamicBoostSet => EnableDynamicThreadPriorityBoost.HasValue;
        [JsonIgnore]
        public bool IsIoPrioritySet => IoPriority != ProcessIoPriority.Unchanged;
        [JsonIgnore]
        public bool IsMemoryPrioritySet => MemoryPriority != ProcessMemoryPriority.Unchanged;
        [JsonIgnore]
        public bool IsGpuPrioritySet => GpuPriority != ProcessGpuPriority.Unchanged;
        [JsonIgnore]
        public bool IsEfficiencyModeSet => EnableEfficiencyMode;
        [JsonIgnore]
        public bool IsKillSet => KillOnStart || KillTreeOnStart;

        [JsonIgnore]
        public string CpuAffinityDisplay
        {
            get
            {
                if (CpuAffinityMask == 0) return "默认";
                int count = 0;
                for (int i = 0; i < 64; i++)
                {
                    if ((CpuAffinityMask & (1L << i)) != 0) count++;
                }
                return $"CPU 核心: {count}";
            }
        }

        [JsonIgnore]
        public string DynamicBoostDisplay => EnableDynamicThreadPriorityBoost.HasValue ? (EnableDynamicThreadPriorityBoost.Value ? "动态提升: 开" : "动态提升: 关") : "默认";

        public static string GetPriorityName(ProcessPriority p)
        {
            return p switch
            {
                ProcessPriority.RealTime => "实时",
                ProcessPriority.High => "高",
                ProcessPriority.AboveNormal => "高于正常",
                ProcessPriority.Normal => "正常",
                ProcessPriority.BelowNormal => "低于正常",
                ProcessPriority.Idle => "空闲",
                ProcessPriority.Unchanged => "默认",
                _ => p.ToString()
            };
        }

        public static string GetIoPriorityName(ProcessIoPriority p)
        {
            return p switch
            {
                ProcessIoPriority.VeryLow => "非常低",
                ProcessIoPriority.Low => "低",
                ProcessIoPriority.Normal => "正常",
                ProcessIoPriority.High => "高",
                ProcessIoPriority.Critical => "关键",
                ProcessIoPriority.Unchanged => "默认",
                _ => p.ToString()
            };
        }

        public static string GetMemoryPriorityName(ProcessMemoryPriority p)
        {
            return p switch
            {
                ProcessMemoryPriority.Lowest => "最低",
                ProcessMemoryPriority.VeryLow => "非常低",
                ProcessMemoryPriority.Low => "低",
                ProcessMemoryPriority.Medium => "中",
                ProcessMemoryPriority.BelowNormal => "低于正常",
                ProcessMemoryPriority.Normal => "正常",
                ProcessMemoryPriority.Unchanged => "默认",
                _ => p.ToString()
            };
        }

        public static string GetGpuPriorityName(ProcessGpuPriority p)
        {
            return p switch
            {
                ProcessGpuPriority.Idle => "空闲",
                ProcessGpuPriority.BelowNormal => "低于正常",
                ProcessGpuPriority.Normal => "正常",
                ProcessGpuPriority.AboveNormal => "高于正常",
                ProcessGpuPriority.High => "高",
                ProcessGpuPriority.Realtime => "实时",
                ProcessGpuPriority.Unchanged => "默认",
                _ => p.ToString()
            };
        }
    }
}
