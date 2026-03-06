using Microsoft.UI.Xaml.Data;
using ProcessBoss.Helpers;
using ProcessBoss.Models;
using System;

namespace ProcessBoss.Converters
{
    public class IoPriorityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ProcessIoPriority p)
            {
                return PriorityHelper.GetIoPriorityName(p);
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
