using Calendar.ViewModels;

namespace Calendar.Views;

public partial class ToDoTaskListView : ContentView
{
    public ToDoTaskListView()
	{
        InitializeComponent();
    }

    public void OnAppearing()
    {
        var toDoListViewModel = this.BindingContext as ToDoTaskListViewModel;
        if (toDoListViewModel != null)
        {
            toDoListViewModel.LoadToDoTasksAsync().ConfigureAwait(false);
        }
    }

    public void OnDisappearing()
    {
        var toDoListViewModel = this.BindingContext as ToDoTaskListViewModel;
        if (toDoListViewModel != null)
        {
            toDoListViewModel.EnsureOrderAsync().ConfigureAwait(false);
        }
    }

    private void listView_ItemDragging(object sender, Syncfusion.Maui.ListView.ItemDraggingEventArgs e)
    {
        var toDoListViewModel = this.BindingContext as ToDoTaskListViewModel;
        if (toDoListViewModel != null)
        {
            toDoListViewModel.OnItemDragging(e);
        }
    }

    //private void Editor_Unfocused(object sender, FocusEventArgs e)
    //{
    //    var toDoListViewModel = this.BindingContext as ToDoTaskListViewModel;
    //    if (toDoListViewModel != null)
    //    {
    //        toDoListViewModel(Unfocused(e.));
    //    }

    //}
}