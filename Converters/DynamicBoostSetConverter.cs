using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ProcessBoss.Converters
{
    public class DynamicBoostSetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                // Only show if disabled (false), as enabled is default
                return !b ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
