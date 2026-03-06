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

        private long _lastRightClickTime;
        private ProcessRule? _lastRightClickItem;

        private void Item_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is ProcessRule rule)
            {
                EditRule(rule);
            }
        }

        private void Item_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is ProcessRule rule)
            {
                // Simple double right click detection
                long now = Environment.TickCount64;
                if (_lastRightClickItem == rule && (now - _lastRightClickTime) < 500)
                {
                    // Reset to avoid triple click triggering again immediately
                    _lastRightClickItem = null;
                    ConfirmDeleteRule(rule);
                }
                else
                {
                    _lastRightClickItem = rule;
                    _lastRightClickTime = now;
                }
            }
        }

        private async void EditRule(ProcessRule rule)
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

        private async void ConfirmDeleteRule(ProcessRule rule)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "删除规则",
                Content = $"确定要删除 \"{rule.ProcessName}\" 的规则吗？",
                PrimaryButtonText = "删除",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
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
