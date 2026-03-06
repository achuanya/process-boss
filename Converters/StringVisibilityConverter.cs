using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ProcessBoss.Converters
{
    public class StringVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? s = value as string;
            bool notEmpty = parameter as string == "NotEmpty";
            
            if (notEmpty)
            {
                return !string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
            }
            return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
