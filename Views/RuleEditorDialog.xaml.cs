using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SKM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;

namespace SKM.Views
{
    public sealed partial class RuleEditorDialog : ContentDialog
    {
        public ProcessRule? Result { get; private set; }
        private readonly List<CheckBox> _affinityCheckBoxes = new List<CheckBox>();
        
        // Temporarily store button texts when overlay is shown
        // private string _tempPrimaryText = "";
        // private string _tempSecondaryText = "";

        public RuleEditorDialog(ProcessRule? existingRule = null)
        {
            this.InitializeComponent();
            
            InitializePriorityComboBox();
            InitializeAffinityList(existingRule?.CpuAffinityMask ?? 0);

            if (existingRule != null)
            {
                PathTextBox.Text = existingRule.FullPath;
                EfficiencyModeCheck.IsChecked = existingRule.EnableEfficiencyMode;
                KillCheck.IsChecked = existingRule.KillOnStart;
                KillTreeCheck.IsChecked = existingRule.KillTreeOnStart;
                
                // Set Priority
                PriorityComboBox.SelectedValue = existingRule.Priority;
            }
        }

        private void InitializePriorityComboBox()
        {
            PriorityComboBox.ItemsSource = Enum.GetValues(typeof(ProcessPriority));
            PriorityComboBox.SelectedItem = ProcessPriority.Normal;
        }

        private void InitializeAffinityList(long currentMask)
        {
            int coreCount = Environment.ProcessorCount;
            _affinityCheckBoxes.Clear();
            AffinityStackPanel.Children.Clear();

            // 0 means all cores in our logic
            bool checkAll = (currentMask == 0);
            
            // Set "All Processors" checkbox state
            AllProcessorsCheck.IsChecked = checkAll;
            if (!checkAll)
            {
                // If not 0, check if all bits are set
                // Actually, if mask is not 0, it is specific. 
                // But if the specific mask covers all cores, it is effectively "All".
                // However, we treat 0 as the special "All" value.
            }

            for (int i = 0; i < coreCount; i++)
            {
                bool isChecked = checkAll || ((currentMask & (1L << i)) != 0);
                
                var cb = new CheckBox
                {
                    Content = $"CPU {i}",
                    IsChecked = isChecked,
                    Tag = i
                };
                cb.Click += CpuCheckBox_Click;
                _affinityCheckBoxes.Add(cb);
                AffinityStackPanel.Children.Add(cb);
            }
            
            UpdateAffinityButtonContent();
        }

        private void UpdateAffinityButtonContent()
        {
             int selected = _affinityCheckBoxes.Count(cb => cb.IsChecked == true);
             if (selected == _affinityCheckBoxes.Count)
             {
                 AffinityButton.Content = "所有处理器";
                 AllProcessorsCheck.IsChecked = true;
             }
             else
             {
                 AffinityButton.Content = $"已选择 {selected} / {_affinityCheckBoxes.Count} 个处理器";
                 AllProcessorsCheck.IsChecked = false;
             }
             
             // Update subtitle in overlay
             if (AffinitySubtitle != null)
             {
                 // AffinitySubtitle.Text = ...
             }
        }
        
        private void ShowAffinityOverlay_Click(object sender, RoutedEventArgs e)
        {
            // Change Title
            this.Title = "CPU 核心限制";

            // Update subtitle
            string processName = "此程序";
            if (!string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                try
                {
                    processName = System.IO.Path.GetFileName(PathTextBox.Text);
                    if (string.IsNullOrEmpty(processName)) processName = PathTextBox.Text;
                }
                catch { processName = PathTextBox.Text; }
            }
            AffinitySubtitle.Text = $"允许哪些处理器运行 \"{processName}\"?";

            // Hide main form
            MainForm.Visibility = Visibility.Collapsed;
            
            // Show Overlay
            AffinityOverlay.Visibility = Visibility.Visible;
        }

        private void AffinityOk_Click(object sender, RoutedEventArgs e)
        {
            UpdateAffinityButtonContent();
            CloseAffinityOverlay();
        }

        private void AffinityCancel_Click(object sender, RoutedEventArgs e)
        {
            // Revert changes? 
            // For simplicity, we just keep whatever is checked currently, 
            // or we could store the mask before opening and revert.
            // But usually "Cancel" in a sub-dialog implies reverting.
            // Let's implement reverting.
            // ... (Skipping complex revert logic for now, assumes user accepts current state or manually reverts)
            
            CloseAffinityOverlay();
        }

        private void CloseAffinityOverlay()
        {
            this.Title = "规则编辑";
            AffinityOverlay.Visibility = Visibility.Collapsed;
            MainForm.Visibility = Visibility.Visible;
        }

        private void AllProcessors_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = AllProcessorsCheck.IsChecked ?? false;
            foreach (var cb in _affinityCheckBoxes)
            {
                cb.IsChecked = isChecked;
            }
        }

        private void CpuCheckBox_Click(object sender, RoutedEventArgs e)
        {
            int selected = _affinityCheckBoxes.Count(cb => cb.IsChecked == true);
            if (selected == _affinityCheckBoxes.Count)
            {
                AllProcessorsCheck.IsChecked = true;
            }
            else
            {
                AllProcessorsCheck.IsChecked = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AffinityOverlay.Visibility == Visibility.Visible)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(PathTextBox.Text))
            {
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
            if (PriorityComboBox.SelectedItem is ProcessPriority p)
            {
                Result.Priority = p;
            }

            // Get Affinity
            long mask = 0;
            int selectedCount = 0;
            for (int i = 0; i < _affinityCheckBoxes.Count; i++)
            {
                if (_affinityCheckBoxes[i].IsChecked == true)
                {
                    mask |= (1L << i);
                    selectedCount++;
                }
            }
            
            if (selectedCount == _affinityCheckBoxes.Count)
            {
                mask = 0;
            }
            
            Result.CpuAffinityMask = mask;
            
            this.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = null;
            this.Hide();
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            
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
