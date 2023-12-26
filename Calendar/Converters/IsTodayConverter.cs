using System.Globalization;

namespace Calendar.Converters;

internal class IsTodayConverter : BindableObject, IValueConverter
{
    public IsTodayConverter()
    {
    }

    public static readonly BindableProperty TrueColorProperty =
        BindableProperty.Create(nameof(TrueColor), typeof(Color), typeof(IsTodayConverter), null, BindingMode.OneWay, null, null);

    public static readonly BindableProperty FalseColorProperty =
        BindableProperty.Create(nameof(FalseColor), typeof(Color), typeof(IsTodayConverter), null, BindingMode.OneWay, null, null);

    public Color TrueColor
    {
        get { return (Color)GetValue(TrueColorProperty); }
        set { SetValue(TrueColorProperty, value); }
    }

    public Color FalseColor
    {
        get { return (Color)GetValue(FalseColorProperty); }
        set { SetValue(FalseColorProperty, value); }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string today = DateTime.Today.ToString("MMM dd, yyyy, ddd");
        if (value.Equals(today))
        {
            return TrueColor!;
        }
        else
        {
            return FalseColor!;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
