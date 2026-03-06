using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ProcessBoss.Converters
{
    public class CpuAffinitySetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long mask)
            {
                return mask != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
