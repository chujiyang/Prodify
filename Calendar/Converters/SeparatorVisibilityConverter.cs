using Syncfusion.Maui.ListView;
using System.Globalization;

namespace Calendar.Converters;

internal class SeparatorVisibilityConverter :  IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var listView = parameter as SfListView;

        if (value == null || listView == null || listView.DataSource.DisplayItems.Count <= 0)
        {
            return false;
        }

        return listView.DataSource.DisplayItems[listView.DataSource.DisplayItems.Count - 1] != value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
