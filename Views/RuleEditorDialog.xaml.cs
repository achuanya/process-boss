using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProcessBoss.Helpers;
using ProcessBoss.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;

namespace ProcessBoss.Views
{
    public sealed partial class RuleEditorDialog : ContentDialog
    {
        public ProcessRule? Result { get; private set; }
        private readonly List<CheckBox> _affinityCheckBoxes = new List<CheckBox>();
        private bool _originalIsEnabled = true;
        
        public RuleEditorDialog(ProcessRule? existingRule = null)
        {
            this.InitializeComponent();
            
            InitializePriorityComboBox();
            InitializeAffinityList(existingRule?.CpuAffinityMask ?? 0);

            if (existingRule != null)
            {
                this.Title = "规则编辑";
                _originalIsEnabled = existingRule.IsEnabled;
                PathTextBox.Text = existingRule.FullPath;
                RemarksTextBox.Text = existingRule.Remarks;
                EfficiencyModeCheck.IsChecked = existingRule.EnableEfficiencyMode;
                DynamicBoostCheck.IsChecked = existingRule.EnableDynamicThreadPriorityBoost;
                KillCheck.IsChecked = existingRule.KillOnStart;
                KillTreeCheck.IsChecked = existingRule.KillTreeOnStart;
                
                // Set Priorities
                SetComboSelection(PriorityComboBox, existingRule.Priority);
                SetComboSelection(IoPriorityComboBox, existingRule.IoPriority);
                SetComboSelection(MemoryPriorityComboBox, existingRule.MemoryPriority);
                SetComboSelection(GpuPriorityComboBox, existingRule.GpuPriority);
            }
            else
            {
                this.Title = "添加规则";
                DynamicBoostCheck.IsChecked = true; // Default to enabled
            }
        }

        private void SetComboSelection<T>(ComboBox comboBox, T value)
        {
             if (comboBox.ItemsSource == null) return;
             
             var items = comboBox.ItemsSource as IEnumerable<ComboItem<T>>;
             if (items == null) return;

             foreach (var item in items)
             {
                 if (EqualityComparer<T>.Default.Equals(item.Value, value))
                 {
                     comboBox.SelectedItem = item;
                     break;
                 }
             }
        }

        private void InitializePriorityComboBox()
        {
            PriorityComboBox.ItemsSource = PriorityDataHelper.GetProcessPriorities();
            PriorityComboBox.SelectedIndex = 0;

            IoPriorityComboBox.ItemsSource = PriorityDataHelper.GetIoPriorities();
            IoPriorityComboBox.SelectedIndex = 0;

            MemoryPriorityComboBox.ItemsSource = PriorityDataHelper.GetMemoryPriorities();
            MemoryPriorityComboBox.SelectedIndex = 0;

            GpuPriorityComboBox.ItemsSource = PriorityDataHelper.GetGpuPriorities();
            GpuPriorityComboBox.SelectedIndex = 0;
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
                IsEnabled = _originalIsEnabled,
                FullPath = PathTextBox.Text,
                ProcessName = System.IO.Path.GetFileName(PathTextBox.Text),
                Remarks = RemarksTextBox.Text,
                EnableEfficiencyMode = EfficiencyModeCheck.IsChecked ?? false,
                EnableDynamicThreadPriorityBoost = DynamicBoostCheck.IsChecked ?? true,
                KillOnStart = KillCheck.IsChecked ?? false,
                KillTreeOnStart = KillTreeCheck.IsChecked ?? false
            };

            // Get Priority
            if (PriorityComboBox.SelectedItem is ComboItem<ProcessPriority> p)
            {
                Result.Priority = p.Value;
            }

            if (IoPriorityComboBox.SelectedItem is ComboItem<ProcessIoPriority> io)
            {
                Result.IoPriority = io.Value;
            }

            if (MemoryPriorityComboBox.SelectedItem is ComboItem<ProcessMemoryPriority> mem)
            {
                Result.MemoryPriority = mem.Value;
            }

            if (GpuPriorityComboBox.SelectedItem is ComboItem<ProcessGpuPriority> gpu)
            {
                Result.GpuPriority = gpu.Value;
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
