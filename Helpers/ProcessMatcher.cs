using System;
using System.Diagnostics;
using ProcessBoss.Models;

namespace ProcessBoss.Helpers
{
    public static class ProcessMatcher
    {
        public static bool IsMatch(Process process, ProcessRule rule)
        {
            try
            {
                // 1. Name Match
                // Process.ProcessName does not include extension, but rule.ProcessName usually does (e.g. "notepad.exe")
                // We should handle both cases or standardize.
                // In MonitorService, it was doing:
                // string pNameNoExt = Path.GetFileNameWithoutExtension(rule.ProcessName);
                // Process.GetProcessesByName(pNameNoExt);
                // So the process passed here should have the same name.
                
                // Let's check name just in case
                string pName = process.ProcessName;
                string ruleName = rule.ProcessName;
                
                if (ruleName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ruleName.Equals(pName + ".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!ruleName.Equals(pName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                // 2. Path Match (Optional)
                if (!string.IsNullOrEmpty(rule.FullPath))
                {
                    string? processPath = null;
                    try
                    {
                        processPath = process.MainModule?.FileName;
                    }
                    catch 
                    {
                        // Access Denied or other error.
                        // If we can't read path, but user specified a path, strict matching fails?
                        // Or we assume it's the right one if name matches?
                        // Original logic: "Ignore access denied, assume match by name" implies if path is null, we continue (return true).
                        // BUT: "if (processPath != null && !processPath.Equals...)"
                        // This means if processPath IS null (access denied), we DO NOT check equality, so we return true (Match).
                        return true; 
                    }

                    if (processPath != null && !processPath.Equals(rule.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
