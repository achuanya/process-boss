using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ProcessBoss.Models;
using System;

namespace ProcessBoss.Converters
{
    public class MemoryPrioritySetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ProcessMemoryPriority p)
            {
                return p != ProcessMemoryPriority.Unchanged ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
