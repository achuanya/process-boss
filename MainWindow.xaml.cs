using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ProcessBoss.Models;
using ProcessBoss.Services;
using ProcessBoss.Views;
using System;
using System.Collections.ObjectModel;
using Windows.Graphics;

namespace ProcessBoss
{
    public sealed partial class MainWindow : Window
    {
        private readonly ConfigService _configService;
        private readonly MonitorService _monitorService;
        private AppWindow _appWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            _configService = new ConfigService();
            _monitorService = new MonitorService(_configService);

            InitializeWindow();
            
            // Default selection
            NavView.SelectedItem = NavView.MenuItems[0];
            // Manually trigger navigation because setting SelectedItem doesn't fire SelectionChanged automatically
            if (NavView.SelectedItem is NavigationViewItem)
            {
                 ContentFrame.Navigate(typeof(RulesPage));
                 if (ContentFrame.Content is RulesPage rulesPage)
                 {
                     rulesPage.Initialize(_configService, _monitorService);
                 }
            }
        }

        private void InitializeWindow()
        {
            // Get AppWindow
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(wndId);

            if (_appWindow != null)
            {
                // Resize to 1350x900
                _appWindow.Resize(new SizeInt32(1350, 900));

                // Disable Resizing and Maximizing
                if (_appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsResizable = true;
                    presenter.IsMaximizable = true;
                    
                    // Hide system title bar to use custom buttons
                    try 
                    {
                        presenter.SetBorderAndTitleBar(true, false);
                    }
                    catch { /* Fallback for older versions if needed */ }
                }

                _appWindow.Changed += AppWindow_Changed;
                _appWindow.Closing += AppWindow_Closing;
            }

            // Extend content into title bar
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar); 
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            // Ensure we stop monitoring and restore processes before closing
            _monitorService.Stop();
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // Update icon on any relevant change (size or presenter state)
            UpdateMaximizeButtonIcon();
        }

        private void UpdateMaximizeButtonIcon()
        {
            if (_appWindow.Presenter is OverlappedPresenter p)
            {
                if (MaximizeButton.Content is FontIcon icon)
                {
                    if (p.State == OverlappedPresenterState.Maximized)
                    {
                        icon.Glyph = "\uE923"; // ChromeRestore
                    }
                    else
                    {
                        icon.Glyph = "\uE922"; // ChromeMaximize
                    }
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Settings Page
            }
            else if (args.SelectedItem is NavigationViewItem item)
            {
                string tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "Rules":
                        ContentFrame.Navigate(typeof(RulesPage));
                        if (ContentFrame.Content is RulesPage rulesPage)
                        {
                            rulesPage.Initialize(_configService, _monitorService);
                        }
                        break;
                    case "Processes":
                        ContentFrame.Navigate(typeof(ProcessesPage));
                        if (ContentFrame.Content is ProcessesPage procPage)
                        {
                            procPage.Initialize(_configService, _monitorService);
                        }
                        break;
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer?.Tag?.ToString() == "ToggleMonitor")
            {
                ToggleMonitor();
            }
        }

        private void ToggleMonitor()
        {
            if (_monitorService.IsRunning)
            {
                _monitorService.Stop();
                
                MonitorText.Text = "开始调教";
                MonitorPlayIcon.Visibility = Visibility.Visible;
                MonitorRunningIcon.Visibility = Visibility.Collapsed;
                MonitorBallAnimation.Stop();
            }
            else
            {
                _monitorService.Start();
                
                MonitorText.Text = "停止调教";
                MonitorPlayIcon.Visibility = Visibility.Collapsed;
                MonitorRunningIcon.Visibility = Visibility.Visible;
                MonitorBallAnimation.Begin();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appWindow.Presenter is OverlappedPresenter p)
            {
                p.Minimize();
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appWindow.Presenter is OverlappedPresenter p)
            {
                if (p.State == OverlappedPresenterState.Maximized)
                {
                    p.Restore();
                }
                else
                {
                    p.Maximize();
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
