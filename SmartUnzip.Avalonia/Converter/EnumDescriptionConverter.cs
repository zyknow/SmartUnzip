using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Bing.Extensions;

namespace SmartUnzip.Avalonia.Converter;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // return value;
        if (value is Enum enumValue)
        {
            var type = enumValue.GetType();
            var desc = enumValue.Description();
            return desc;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var items = Bing.Helpers.Enum.GetItems(targetType);
        var item = items.FirstOrDefault(x => x.Text == (string)value);
        return item.Value;
    }
}