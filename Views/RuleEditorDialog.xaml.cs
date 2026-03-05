using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SKM.Models;
using System;
using System.Linq;
using Windows.Storage.Pickers;

namespace SKM.Views
{
    public sealed partial class RuleEditorDialog : ContentDialog
    {
        public ProcessRule? Result { get; private set; }

        public RuleEditorDialog(ProcessRule? existingRule = null)
        {
            this.InitializeComponent();
            
            if (existingRule != null)
            {
                PathTextBox.Text = existingRule.FullPath;
                EfficiencyModeCheck.IsChecked = existingRule.EnableEfficiencyMode;
                KillCheck.IsChecked = existingRule.KillOnStart;
                KillTreeCheck.IsChecked = existingRule.KillTreeOnStart;
                
                // Set Priority
                foreach (RadioButton rb in PriorityRadioButtons.Items)
                {
                    if (rb.Tag.ToString() == existingRule.Priority.ToString())
                    {
                        rb.IsChecked = true;
                        break;
                    }
                }

                // Set Affinity (Simplified for demo)
                long mask = existingRule.CpuAffinityMask;
                if (mask == 0) mask = -1; // -1 means all

                Cpu0Check.IsChecked = (mask & 1) != 0;
                Cpu1Check.IsChecked = (mask & 2) != 0;
                Cpu2Check.IsChecked = (mask & 4) != 0;
                Cpu3Check.IsChecked = (mask & 8) != 0;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                args.Cancel = true; // Validate
                return;
            }

            Result = new ProcessRule
            {
                FullPath = PathTextBox.Text,
                ProcessName = System.IO.Path.GetFileName(PathTextBox.Text),
                EnableEfficiencyMode = EfficiencyModeCheck.IsChecked ?? false,
                KillOnStart = KillCheck.IsChecked ?? false,
                KillTreeOnStart = KillTreeCheck.IsChecked ?? false
            };

            // Get Priority
            if (PriorityRadioButtons.SelectedItem is RadioButton rb && rb.Tag is string tag)
            {
                if (Enum.TryParse<ProcessPriority>(tag, out var p))
                {
                    Result.Priority = p;
                }
            }

            // Get Affinity
            long mask = 0;
            if (Cpu0Check.IsChecked == true) mask |= 1;
            if (Cpu1Check.IsChecked == true) mask |= 2;
            if (Cpu2Check.IsChecked == true) mask |= 4;
            if (Cpu3Check.IsChecked == true) mask |= 8;
            
            // If all checked (simplified logic for demo), maybe 0? 
            // But let's keep the mask explicit.
            Result.CpuAffinityMask = mask;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            
            // WinUI 3 Window Handle required
            var window = (Application.Current as App)?.Window;
            if (window != null)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
            }

            picker.FileTypeFilter.Add(".exe");
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                PathTextBox.Text = file.Path;
            }
        }
    }
}
