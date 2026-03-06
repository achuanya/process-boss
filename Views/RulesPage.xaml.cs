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
            this.Unloaded += RulesPage_Unloaded;
        }

        private void RulesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_configService != null)
            {
                _configService.RuleAdded -= OnRuleAdded;
                _configService.RuleRemoved -= OnRuleRemoved;
                _configService.RuleUpdated -= OnRuleUpdated;
            }
        }

        public void Initialize(ConfigService configService, MonitorService monitorService)
        {
            _configService = configService;
            _monitorService = monitorService;

            _configService.RuleAdded += OnRuleAdded;
            _configService.RuleRemoved += OnRuleRemoved;
            _configService.RuleUpdated += OnRuleUpdated;

            LoadRules();
        }

        private void OnRuleAdded(ProcessRule rule)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                Rules.Add(rule);
            });
        }

        private void OnRuleRemoved(ProcessRule rule)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                Rules.Remove(rule);
            });
        }

        private void OnRuleUpdated(ProcessRule rule)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                int index = Rules.IndexOf(rule);
                if (index != -1)
                {
                    Rules[index] = rule;
                }
            });
        }

        private void LoadRules()
        {
            Rules.Clear();
            foreach (var rule in _configService.Rules)
            {
                Rules.Add(rule);
            }
        }

        // AddButton_Click will be removed later as the button is moving to MainWindow
        // For now we keep the event handler signature if XAML still references it, 
        // but since we are removing the XAML button in the next step, we can remove this method too 
        // or just keep it empty if needed for compilation during transition.
        // Actually I will remove the method entirely when I update XAML.
        
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
