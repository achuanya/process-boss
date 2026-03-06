using Microsoft.UI.Xaml.Data;
using ProcessBoss.Helpers;
using ProcessBoss.Models;
using System;

namespace ProcessBoss.Converters
{
    public class ProcessPriorityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ProcessPriority p)
            {
                return PriorityHelper.GetPriorityName(p);
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
