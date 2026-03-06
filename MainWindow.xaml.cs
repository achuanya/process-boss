using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SKM.Models;
using SKM.Services;
using SKM.Views;
using System;
using System.Collections.ObjectModel;
using Windows.Graphics;

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

            InitializeWindow();
            LoadRules();
        }

        private void InitializeWindow()
        {
            // Get AppWindow
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(wndId);

            if (appWindow != null)
            {
                // Resize to 1350x900
                appWindow.Resize(new SizeInt32(1350, 900));

                // Disable Resizing and Maximizing
                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsResizable = true;
                    presenter.IsMaximizable = true;
                }
            }

            // Extend content into title bar
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar); // We will add a Grid named AppTitleBar in XAML
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

        private void MonitorSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (MonitorSwitch.IsOn)
            {
                _monitorService.Start();
                StatusTextBlock.Text = "监控中...";
            }
            else
            {
                _monitorService.Stop();
                StatusTextBlock.Text = "已停止监控";
            }
        }
    }
}