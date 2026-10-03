using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace MPDCtrlX.Common;

public class BoolToFontWeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // If the bool is true, return Bold, otherwise Normal
        if (value is bool isBold && isBold)
        {
            return FontWeight.SemiBold;
        }
        return FontWeight.Normal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

