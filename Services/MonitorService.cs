using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SKM.Models;

namespace SKM.Services
{
    public class MonitorService : IDisposable
    {
        private ManagementEventWatcher? _startWatcher;
        private readonly ConfigService _configService;
        private readonly ProcessOptimizer _optimizer;
        private bool _isRunning;

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
        }

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

                if (matchedRule != null)
                {
                    try
                    {
                        var process = Process.GetProcessById(processId);
                        if (!string.IsNullOrEmpty(matchedRule.FullPath))
                        {
                            try 
                            {
                                if (process.MainModule?.FileName != null && 
                                    !process.MainModule.FileName.Equals(matchedRule.FullPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    return; 
                                }
                            }
                            catch { /* access denied */ }
                        }

                        _optimizer.ApplyRule(process, matchedRule);
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

                if (matchedRule != null)
                {
                     if (!string.IsNullOrEmpty(matchedRule.FullPath))
                     {
                         try
                         {
                             if (p.MainModule?.FileName != null && !p.MainModule.FileName.Equals(matchedRule.FullPath, StringComparison.OrdinalIgnoreCase))
                             {
                                 return;
                             }
                         }
                         catch { return; }
                     }
                     _optimizer.ApplyRule(p, matchedRule);
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
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
