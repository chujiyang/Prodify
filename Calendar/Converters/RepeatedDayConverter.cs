using Calendar.Extensions;
using Calendar.ViewModels;
using Syncfusion.Maui.ListView;
using System.Globalization;

namespace Calendar.Converters;

internal class RepeatedDayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var listview = parameter as SfListView;
        var index = listview.DataSource.DisplayItems.IndexOf(value);
        if (index > 0)
        {
            var previousRow = listview.DataSource.DisplayItems[index-1];
            return (previousRow as EventViewModel)?.From.ToShortestDateTime() == (value as EventViewModel).From.ToShortestDateTime() ? string.Empty : (value as EventViewModel)?.From.ToString("dd MMM, ddd");
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}