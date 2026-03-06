using System.Collections.Generic;
using ProcessBoss.Models;

namespace ProcessBoss.Helpers
{
    public static class PriorityDataHelper
    {
        public static List<ComboItem<ProcessPriority>> GetProcessPriorities()
        {
            return new List<ComboItem<ProcessPriority>>
            {
                new ComboItem<ProcessPriority> { Value = ProcessPriority.Unchanged, Display = PriorityHelper.GetPriorityName(ProcessPriority.Unchanged) },
                new ComboItem<ProcessPriority> { Value = ProcessPriority.RealTime, Display = PriorityHelper.GetPriorityName(ProcessPriority.RealTime) },
                new ComboItem<ProcessPriority> { Value = ProcessPriority.High, Display = PriorityHelper.GetPriorityName(ProcessPriority.High) },
                new ComboItem<ProcessPriority> { Value = ProcessPriority.AboveNormal, Display = PriorityHelper.GetPriorityName(ProcessPriority.AboveNormal) },
                new ComboItem<ProcessPriority> { Value = ProcessPriority.Normal, Display = PriorityHelper.GetPriorityName(ProcessPriority.Normal) },
                new ComboItem<ProcessPriority> { Value = ProcessPriority.BelowNormal, Display = PriorityHelper.GetPriorityName(ProcessPriority.BelowNormal) },
                new ComboItem<ProcessPriority> { Value = ProcessPriority.Idle, Display = PriorityHelper.GetPriorityName(ProcessPriority.Idle) }
            };
        }

        public static List<ComboItem<ProcessIoPriority>> GetIoPriorities()
        {
            return new List<ComboItem<ProcessIoPriority>>
            {
                new ComboItem<ProcessIoPriority> { Value = ProcessIoPriority.Unchanged, Display = PriorityHelper.GetIoPriorityName(ProcessIoPriority.Unchanged) },
                new ComboItem<ProcessIoPriority> { Value = ProcessIoPriority.Critical, Display = PriorityHelper.GetIoPriorityName(ProcessIoPriority.Critical) },
                new ComboItem<ProcessIoPriority> { Value = ProcessIoPriority.High, Display = PriorityHelper.GetIoPriorityName(ProcessIoPriority.High) },
                new ComboItem<ProcessIoPriority> { Value = ProcessIoPriority.Normal, Display = PriorityHelper.GetIoPriorityName(ProcessIoPriority.Normal) },
                new ComboItem<ProcessIoPriority> { Value = ProcessIoPriority.Low, Display = PriorityHelper.GetIoPriorityName(ProcessIoPriority.Low) },
                new ComboItem<ProcessIoPriority> { Value = ProcessIoPriority.VeryLow, Display = PriorityHelper.GetIoPriorityName(ProcessIoPriority.VeryLow) }
            };
        }

        public static List<ComboItem<ProcessMemoryPriority>> GetMemoryPriorities()
        {
            return new List<ComboItem<ProcessMemoryPriority>>
            {
                new ComboItem<ProcessMemoryPriority> { Value = ProcessMemoryPriority.Unchanged, Display = PriorityHelper.GetMemoryPriorityName(ProcessMemoryPriority.Unchanged) },
                new ComboItem<ProcessMemoryPriority> { Value = ProcessMemoryPriority.Normal, Display = PriorityHelper.GetMemoryPriorityName(ProcessMemoryPriority.Normal) },
                new ComboItem<ProcessMemoryPriority> { Value = ProcessMemoryPriority.BelowNormal, Display = PriorityHelper.GetMemoryPriorityName(ProcessMemoryPriority.BelowNormal) },
                new ComboItem<ProcessMemoryPriority> { Value = ProcessMemoryPriority.Medium, Display = PriorityHelper.GetMemoryPriorityName(ProcessMemoryPriority.Medium) },
                new ComboItem<ProcessMemoryPriority> { Value = ProcessMemoryPriority.Low, Display = PriorityHelper.GetMemoryPriorityName(ProcessMemoryPriority.Low) },
                new ComboItem<ProcessMemoryPriority> { Value = ProcessMemoryPriority.VeryLow, Display = PriorityHelper.GetMemoryPriorityName(ProcessMemoryPriority.VeryLow) },
                new ComboItem<ProcessMemoryPriority> { Value = ProcessMemoryPriority.Lowest, Display = PriorityHelper.GetMemoryPriorityName(ProcessMemoryPriority.Lowest) }
            };
        }

        public static List<ComboItem<ProcessGpuPriority>> GetGpuPriorities()
        {
            return new List<ComboItem<ProcessGpuPriority>>
            {
                new ComboItem<ProcessGpuPriority> { Value = ProcessGpuPriority.Unchanged, Display = PriorityHelper.GetGpuPriorityName(ProcessGpuPriority.Unchanged) },
                new ComboItem<ProcessGpuPriority> { Value = ProcessGpuPriority.Realtime, Display = PriorityHelper.GetGpuPriorityName(ProcessGpuPriority.Realtime) },
                new ComboItem<ProcessGpuPriority> { Value = ProcessGpuPriority.High, Display = PriorityHelper.GetGpuPriorityName(ProcessGpuPriority.High) },
                new ComboItem<ProcessGpuPriority> { Value = ProcessGpuPriority.AboveNormal, Display = PriorityHelper.GetGpuPriorityName(ProcessGpuPriority.AboveNormal) },
                new ComboItem<ProcessGpuPriority> { Value = ProcessGpuPriority.Normal, Display = PriorityHelper.GetGpuPriorityName(ProcessGpuPriority.Normal) },
                new ComboItem<ProcessGpuPriority> { Value = ProcessGpuPriority.BelowNormal, Display = PriorityHelper.GetGpuPriorityName(ProcessGpuPriority.BelowNormal) },
                new ComboItem<ProcessGpuPriority> { Value = ProcessGpuPriority.Idle, Display = PriorityHelper.GetGpuPriorityName(ProcessGpuPriority.Idle) }
            };
        }
    }
}
