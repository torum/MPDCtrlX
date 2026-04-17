using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MPDCtrlX.Core.Converters;

/*
public static class TreeViewItemExtensions
{
    public static int GetDepth(this TreeViewItem item)
    {
        TreeViewItem? parent;
        while ((parent = GetParent(item)) is not null)
        {
            return GetDepth(parent) + 1;
        }
        return 0;
    }

    private static TreeViewItem? GetParent(TreeViewItem item)
    {
        var parent = VisualTreeHelper.GetParent(item);
        while (!(parent is TreeViewItem || parent is TreeView))
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as TreeViewItem;
    }
}
*/

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

