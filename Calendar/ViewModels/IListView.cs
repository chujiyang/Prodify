using Microsoft.Maui.Controls;

namespace Calendar.ViewModels
{
    public interface IListView
    {
        void ScrollTo(Object obj, ScrollToPosition position) { }
    }
}
