using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ProcessBoss.Models;

namespace ProcessBoss.Services
{
    public class MonitorService : IDisposable
    {
        private ManagementEventWatcher? _startWatcher;
        private readonly ConfigService _configService;
        private readonly ProcessOptimizer _optimizer;
        private bool _isRunning;
        private readonly HashSet<int> _managedProcessIds = new HashSet<int>();

        // Use a timer as a fallback or complementary check, 
        // because WMI events might be missed or slow, 
        // but for "Instant" reaction, WMI is usually better if it works.
        // However, WMI Win32_ProcessStartTrace requires admin rights often.
        // Let's stick to WMI first, and maybe a slow poll loop for reliability.
        private CancellationTokenSource? _cts;

        public MonitorService(ConfigService configService)
        {
            _configService = configService;
            _optimizer = new ProcessOptimizer();

            _configService.RuleAdded += OnRuleChanged;
            _configService.RuleUpdated += OnRuleChanged;
            _configService.RuleRemoved += OnRuleRemoved;
        }

        private void OnRuleRemoved(ProcessRule rule)
        {
            // When a rule is removed, we should restore defaults to those processes
            RestoreRuleDefaults(rule);
        }

        private void RestoreRuleDefaults(ProcessRule rule)
        {
            Task.Run(() =>
            {
                try
                {
                    string pNameNoExt = System.IO.Path.GetFileNameWithoutExtension(rule.ProcessName);
                    var processes = Process.GetProcessesByName(pNameNoExt);
                    foreach (var p in processes)
                    {
                        try
                        {
                            // Check path if possible, but don't fail if Access Denied
                            if (!string.IsNullOrEmpty(rule.FullPath))
                            {
                                string? processPath = null;
                                try
                                {
                                    processPath = p.MainModule?.FileName;
                                }
                                catch { /* Ignore access denied, assume match by name */ }

                                if (processPath != null && !processPath.Equals(rule.FullPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }
                            _optimizer.RestoreDefaults(p);
                            lock (_managedProcessIds)
                            {
                                _managedProcessIds.Remove(p.Id);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        private void OnRuleChanged(ProcessRule rule)
        {
            if (_isRunning)
            {
                if (rule.IsEnabled)
                {
                    ApplyRuleToRunningProcesses(rule);
                }
                else
                {
                    // Rule disabled -> restore defaults
                    RestoreRuleDefaults(rule);
                }
            }
        }

        private void ApplyRuleToRunningProcesses(ProcessRule rule)
        {
            Task.Run(() =>
            {
                try
                {
                    string pNameNoExt = System.IO.Path.GetFileNameWithoutExtension(rule.ProcessName);
                    var processes = Process.GetProcessesByName(pNameNoExt);
                    foreach (var p in processes)
                    {
                        try
                        {
                            // Check path if possible, but don't fail if Access Denied
                            if (!string.IsNullOrEmpty(rule.FullPath))
                            {
                                string? processPath = null;
                                try
                                {
                                    processPath = p.MainModule?.FileName;
                                }
                                catch { /* Ignore access denied, assume match by name */ }

                                if (processPath != null && !processPath.Equals(rule.FullPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }

                            _optimizer.ApplyRule(p, rule);
                            lock (_managedProcessIds)
                            {
                                _managedProcessIds.Add(p.Id);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        public bool IsRunning => _isRunning;

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            try
            {
                // WMI approach
                // Win32_ProcessStartTrace is efficient
                var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                _startWatcher = new ManagementEventWatcher(query);
                _startWatcher.EventArrived += OnProcessStarted;
                _startWatcher.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WMI Monitor failed to start: {ex.Message}. Falling back to polling.");
                // Fallback to polling if WMI fails (e.g. permission issues)
                StartPolling();
            }
        }

        private void StartPolling()
        {
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                var knownIds = new HashSet<int>();
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var processes = Process.GetProcesses();
                        var currentIds = new HashSet<int>(processes.Select(p => p.Id));

                        foreach (var p in processes)
                        {
                            if (!knownIds.Contains(p.Id))
                            {
                                // New process
                                CheckAndApply(p);
                                knownIds.Add(p.Id);
                            }
                        }
                        
                        // Cleanup dead IDs
                        knownIds.IntersectWith(currentIds);
                    }
                    catch { }
                    
                    await Task.Delay(1000, _cts.Token);
                }
            });
        }

        private void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var processNameObj = e.NewEvent.Properties["ProcessName"].Value;
                if (processNameObj == null) return;
                
                string processName = processNameObj.ToString() ?? "";
                int processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);

                // ProcessName in WMI usually includes extension (e.g. "notepad.exe")
                var matchedRule = _configService.Rules.FirstOrDefault(r => 
                    r.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

                if (matchedRule != null && matchedRule.IsEnabled)
                {
                    try
                    {
                        var process = Process.GetProcessById(processId);
                        
                        // Check path if possible
                        if (!string.IsNullOrEmpty(matchedRule.FullPath))
                        {
                            string? processPath = null;
                            try 
                            {
                                processPath = process.MainModule?.FileName;
                            }
                            catch { /* access denied */ }

                            if (processPath != null && !processPath.Equals(matchedRule.FullPath, StringComparison.OrdinalIgnoreCase))
                            {
                                return; 
                            }
                        }

                        _optimizer.ApplyRule(process, matchedRule);
                        lock (_managedProcessIds)
                        {
                            _managedProcessIds.Add(processId);
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in WMI handler: {ex.Message}");
            }
        }

        private void CheckAndApply(Process p)
        {
            try
            {
                // Process.ProcessName does NOT include extension
                string pName = p.ProcessName + ".exe";
                
                var matchedRule = _configService.Rules.FirstOrDefault(r => 
                    r.ProcessName.Equals(pName, StringComparison.OrdinalIgnoreCase) || 
                    r.ProcessName.Equals(p.ProcessName, StringComparison.OrdinalIgnoreCase));

                if (matchedRule != null && matchedRule.IsEnabled)
                {
                     // Check path if possible
                     if (!string.IsNullOrEmpty(matchedRule.FullPath))
                     {
                         string? processPath = null;
                         try
                         {
                             processPath = p.MainModule?.FileName;
                         }
                         catch { /* Access denied */ }

                         if (processPath != null && !processPath.Equals(matchedRule.FullPath, StringComparison.OrdinalIgnoreCase))
                         {
                             return;
                         }
                     }
                     _optimizer.ApplyRule(p, matchedRule);
                     lock (_managedProcessIds)
                     {
                         _managedProcessIds.Add(p.Id);
                     }
                }
            }
            catch { }
        }

        public void Stop()
        {
            _isRunning = false;
            _startWatcher?.Stop();
            _startWatcher?.Dispose();
            _startWatcher = null;
            
            _cts?.Cancel();
            _cts = null;

            // Restore defaults for all managed processes
            RestoreAllDefaults();
        }

        private void RestoreAllDefaults()
        {
            int[] pids;
            lock (_managedProcessIds)
            {
                pids = _managedProcessIds.ToArray();
                _managedProcessIds.Clear();
            }

            foreach (var pid in pids)
            {
                try
                {
                    var p = Process.GetProcessById(pid);
                    _optimizer.RestoreDefaults(p);
                }
                catch { /* Process likely exited */ }
            }
        }

        public void Dispose()
        {
            Stop();
            _configService.RuleAdded -= OnRuleChanged;
            _configService.RuleUpdated -= OnRuleChanged;
            _configService.RuleRemoved -= OnRuleRemoved;
        }
    }
}
