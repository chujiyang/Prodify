using System.Globalization;

namespace Calendar.Converters;

internal class TimespanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DateTime.Today.Add((TimeSpan)(value)).ToString("hh:mm tt");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Converting back rarely happens, a lot of the converters will throw an exception
        throw new NotImplementedException();
    }
}
