using System;

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
}
