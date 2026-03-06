using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProcessBoss.Models;
using ProcessBoss.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessBoss.Views
{
    public sealed partial class ProcessesPage : Page
    {
        public ObservableCollection<ProcessItem> Processes { get; } = new ObservableCollection<ProcessItem>();
        private ConfigService _configService;
        private MonitorService _monitorService;

        public ProcessesPage()
        {
            this.InitializeComponent();
        }

        public void Initialize(ConfigService configService, MonitorService monitorService)
        {
            _configService = configService;
            _monitorService = monitorService;
            RefreshProcesses();
        }

        private async void RefreshProcesses()
        {
            Processes.Clear();
            StatusTextBlock.Text = "正在加载进程...";

            await Task.Run(() =>
            {
                var processList = Process.GetProcesses().Select(p =>
                {
                    try
                    {
                        var path = GetProcessPath(p);
                        return new ProcessItem
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            Path = path,
                            CpuUsage = 0
                        };
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(p => p != null)
                .OrderBy(p => p.Name)
                .ToList();

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var p in processList)
                    {
                        Processes.Add(p);
                    }
                    StatusTextBlock.Text = $"共 {Processes.Count} 个进程";
                });
            });
        }

        private string GetProcessPath(Process p)
        {
            try
            {
                return p.MainModule?.FileName ?? "";
            }
            catch
            {
                return "无法访问";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcesses();
        }

        private async void AddRuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessItem processItem)
            {
                var rule = new ProcessRule
                {
                    ProcessName = processItem.Name + ".exe", 
                    FullPath = processItem.Path == "无法访问" ? "" : processItem.Path
                };

                var dialog = new RuleEditorDialog(rule);
                dialog.XamlRoot = this.Content.XamlRoot;
                var result = await dialog.ShowAsync();

                if (dialog.Result != null)
                {
                    var newRule = dialog.Result;
                    _configService.AddRule(newRule);
                }
            }
        }
    }

    public class ProcessItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public double CpuUsage { get; set; }
    }
}
