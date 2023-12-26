using System.Globalization;

namespace Calendar.Converters;

internal class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !((bool)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;

        // Converting back rarely happens, a lot of the converters will throw an exception
        //throw new NotImplementedException();
    }
}
