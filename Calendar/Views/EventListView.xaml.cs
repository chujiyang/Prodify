using Calendar.ViewModels;

namespace Calendar.Views;

public partial class EventListView : ContentView
{
	public EventListView()
	{
		InitializeComponent();
	}

    public void OnAppearing()
    {
        var eventListViewModel = this.BindingContext as EventListViewModel;
        if (eventListViewModel != null)
        {
            eventListViewModel.ListView = new ListViewAdaptor(this.listView);
            var loadTask = eventListViewModel.LoadEventsAsync();
            loadTask.ConfigureAwait(false);
        }
    }

    public void OnDisappearing()
    {
    }
}