using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ProcessBoss.Models;
using ProcessBoss.Services;
using System;
using System.Collections.ObjectModel;

namespace ProcessBoss.Views
{
    public sealed partial class RulesPage : Page
    {
        public ObservableCollection<ProcessRule> Rules { get; } = new ObservableCollection<ProcessRule>();
        private ConfigService _configService;
        private MonitorService _monitorService;

        public RulesPage()
        {
            this.InitializeComponent();
        }

        public void Initialize(ConfigService configService, MonitorService monitorService)
        {
            _configService = configService;
            _monitorService = monitorService;
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

            if (dialog.Result != null)
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

                if (dialog.Result != null)
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

        private void RuleToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts && ts.Tag is ProcessRule rule)
            {
                // IsEnabled is updated via TwoWay binding, but we need to notify ConfigService
                // to trigger MonitorService update and save to disk.
                // We pass the same object as old and new, ConfigService handles it by index.
                _configService.UpdateRule(rule, rule);
            }
        }
    }
}
