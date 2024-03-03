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


    public bool IsLazyLoading
    {
        get { return _listView.IsLazyLoading; }
        set { _listView.IsLazyLoading = value; }
    }

    public LoadMorePosition LoadMorePosition
    {
        get { return _listView.LoadMorePosition; }
    }

    public void ScrollTo(Object obj, ScrollToPosition position)
    {
        if (obj != null)
        {
            var index = _listView.DataSource.DisplayItems.IndexOf(obj);
            if (index >= 0)
            {
                _listView.ItemsLayout.ScrollToRowIndex(index, position, true);
            }
        }
    }

    public void ScrollToRowIndex(int row)
    {
        _listView.ItemsLayout.ScrollToRowIndex(row, true);
    }

    public void BeginInit()
    {
        _listView.DataSource.BeginInit();
    }

    public void EndInit()
    {
        _listView.DataSource.EndInit();
    }
}
