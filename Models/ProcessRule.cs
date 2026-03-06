using System;
using System.Text.Json.Serialization;

namespace ProcessBoss.Models
{
    public class ProcessRule
    {
        public string ProcessName { get; set; } = string.Empty; // e.g., "notepad.exe"
        public string FullPath { get; set; } = string.Empty; // Optional, for stricter matching
        public string Remarks { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;

        public ProcessPriority Priority { get; set; } = ProcessPriority.Unchanged;
        
        // Bitmask for CPU Affinity. 0 means all cores (or no change).
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

        [JsonIgnore]
        public bool IsKillSet => KillOnStart || KillTreeOnStart;
    }
}
