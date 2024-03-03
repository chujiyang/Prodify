using Syncfusion.Maui.ListView;

namespace Calendar.ViewModels
{

    public interface IListView
    {
        bool IsLazyLoading { get; set; }

        LoadMorePosition LoadMorePosition { get; }

        void ScrollTo(Object obj, ScrollToPosition position);

        void ScrollToRowIndex(int row);

        void BeginInit();
        void EndInit();
    }
}
