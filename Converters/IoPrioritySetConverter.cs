using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ProcessBoss.Models;
using System;

namespace ProcessBoss.Converters
{
    public class IoPrioritySetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ProcessIoPriority p)
            {
                return p != ProcessIoPriority.Unchanged ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
