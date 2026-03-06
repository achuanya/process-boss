using Microsoft.UI.Xaml.Data;
using System;

namespace ProcessBoss.Converters
{
    public class PrefixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            string prefix = parameter as string ?? "";
            return prefix + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
