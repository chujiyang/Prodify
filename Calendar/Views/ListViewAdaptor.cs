using Calendar.ViewModels;
using Syncfusion.Maui.ListView;

namespace Calendar.Views;

public class ListViewAdaptor : IListView
{
    private SfListView _listView;

    public ListViewAdaptor(SfListView listView)
    {
        _listView = listView;
    }
        public void ScrollTo(Object obj)
    {
        if (obj != null)
        {
            var index = _listView.DataSource.DisplayItems.IndexOf(obj);
            if (index >= 0)
            {
                _listView.ItemsLayout.ScrollToRowIndex(index, ScrollToPosition.Start, true);
            }
        }
    }
}
