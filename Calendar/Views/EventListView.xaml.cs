using Calendar.ViewModels;
using Syncfusion.Maui.ListView;
using System.Diagnostics;

namespace Calendar.Views;

public partial class EventListView : ContentView
{

    public EventListView()
    {

        InitializeComponent();

        this.listView.Loaded += ListView_Loaded;
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
    //    Debug.WriteLine(string.Format("SrollY: {0}", e.ScrollY));
    }

    private void ListView_Loaded(object sender, ListViewLoadedEventArgs e)
    {
        var eventListViewModel = this.BindingContext as EventListViewModel;
        if (eventListViewModel != null)
        {
            eventListViewModel.ListView = new ListViewAdaptor(this.listView);
            Task.Run(async () =>
            {
                await eventListViewModel.LoadEventsAsync().ConfigureAwait(false);
            });
        }
    }

    private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        if (!(sender is ScrollView scrollView))
        {
            return;
        }
        
        var scrollSpace = scrollView.ContentSize.Height - scrollView.Height;

        if (e.ScrollY < 10)
        {
            var eventListViewModel = this.BindingContext as EventListViewModel;
            if (eventListViewModel != null)
            {
                eventListViewModel.LoadMoreFromTop();
            }

            return;
        }

        if (scrollSpace > e.ScrollY)
        {
            return;
        }

        Debug.WriteLine(String.Format("Content Height: {0}, View Height:{1}, Scroll space: {2}, ScrollY: {3}",
                scrollView.ContentSize.Height, scrollView.Height, scrollSpace, e.ScrollY));

        {
            // Debug.WriteLine("First line:{0}", visualContainer.ScrollRows.ScrollLineIndex);
            var eventListViewModel = this.BindingContext as EventListViewModel;
            if (eventListViewModel != null)
            {
                eventListViewModel.LoadMoreItems();
            }
        }
    }

}
