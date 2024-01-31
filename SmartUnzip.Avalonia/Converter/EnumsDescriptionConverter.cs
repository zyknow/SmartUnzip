using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SmartUnzip.Avalonia.Converter;

public class EnumsDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 判断是否是可迭代的
        if (value is IEnumerable enumerable && !(value is string))
        {
            var list = new List<string>();
            foreach (var item in enumerable)
            {
                var desc = Bing.Helpers.Enum.GetDescription(item.GetType(),item);
                list.Add(desc);
            }

            return list;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}