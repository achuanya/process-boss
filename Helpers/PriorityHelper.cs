using System;
using ProcessBoss.Models;

namespace ProcessBoss.Helpers
{
    public static class PriorityHelper
    {
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
        
        public static string GetCpuAffinityDisplay(long mask)
        {
            if (mask == 0) return "默认";
            int count = 0;
            for (int i = 0; i < 64; i++)
            {
                if ((mask & (1L << i)) != 0) count++;
            }
            return $"CPU 核心: {count}";
        }

        public static string GetDynamicBoostDisplay(bool? boost)
        {
            return boost.HasValue ? (boost.Value ? "动态提升: 开" : "动态提升: 关") : "默认";
        }
    }
}
