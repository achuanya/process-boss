using Microsoft.UI.Xaml.Data;
using System;

namespace ProcessBoss.Converters
{
    public class CpuAffinityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long mask)
            {
                if (mask == 0) return "默认";
                int count = 0;
                for (int i = 0; i < 64; i++)
                {
                    if ((mask & (1L << i)) != 0) count++;
                }
                return $"CPU 核心: {count}";
            }
            return "默认";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
