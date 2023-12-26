﻿using System.Globalization;

namespace Calendar.Converters;

internal class BoolToGridRowHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((bool)value == true) ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {    // Don't need any convert back
        return null;
    }
}
