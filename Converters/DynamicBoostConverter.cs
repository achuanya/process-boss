using Microsoft.UI.Xaml.Data;
using System;

namespace ProcessBoss.Converters
{
    public class DynamicBoostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return b ? "动态提升: 开" : "动态提升: 关";
            }
            return "默认";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
