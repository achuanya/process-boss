using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SKM.Models;
using SKM.Services;
using SKM.Views;
using System.Collections.ObjectModel;
using System;

namespace SKM
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<ProcessRule> Rules { get; } = new ObservableCollection<ProcessRule>();
        private readonly ConfigService _configService;
        private readonly MonitorService _monitorService;

        public MainWindow()
        {
            this.InitializeComponent();
            _configService = new ConfigService();
            _monitorService = new MonitorService(_configService);

            LoadRules();
        }

        private void LoadRules()
        {
            Rules.Clear();
            foreach (var rule in _configService.Rules)
            {
                Rules.Add(rule);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RuleEditorDialog();
            dialog.XamlRoot = this.Content.XamlRoot;
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Result != null)
            {
                var newRule = dialog.Result;
                _configService.AddRule(newRule);
                Rules.Add(newRule);
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessRule rule)
            {
                var dialog = new RuleEditorDialog(rule);
                dialog.XamlRoot = this.Content.XamlRoot;
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && dialog.Result != null)
                {
                    var updatedRule = dialog.Result;
                    _configService.UpdateRule(rule, updatedRule);
                    
                    int index = Rules.IndexOf(rule);
                    if (index != -1)
                    {
                        Rules[index] = updatedRule;
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProcessRule rule)
            {
                _configService.RemoveRule(rule);
                Rules.Remove(rule);
            }
        }

        private void StartMonitor_Click(object sender, RoutedEventArgs e)
        {
            _monitorService.Start();
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            StatusTextBlock.Text = "监控中...";
        }

        private void StopMonitor_Click(object sender, RoutedEventArgs e)
        {
            _monitorService.Stop();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StatusTextBlock.Text = "已停止监控";
        }
    }
}
