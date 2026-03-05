using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SKM.Models
{
    public enum ProcessPriority
    {
        RealTime,
        High,
        AboveNormal,
        Normal,
        BelowNormal,
        Low,
        Unchanged // Default, do not modify
    }

    public class ProcessRule
    {
        public string ProcessName { get; set; } = string.Empty; // e.g., "notepad.exe"
        public string FullPath { get; set; } = string.Empty; // Optional, for stricter matching
        
        public ProcessPriority Priority { get; set; } = ProcessPriority.Unchanged;
        
        // Bitmask for CPU Affinity. 0 means all cores (or no change).
        // Storing as long to support 64 cores. For >64, we might need a different approach, but long is usually enough for desktop.
        public long CpuAffinityMask { get; set; } = 0; 
        
        public bool EnableEfficiencyMode { get; set; }
        
        public bool KillOnStart { get; set; }
        public bool KillTreeOnStart { get; set; }
    }
}
