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
            toDoListViewModel.ListView = new ListViewAdaptor(this.listView);

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

    private void editor_Focused(object sender, FocusEventArgs e)
    {
        KeyboardPadding.Margin = new Thickness(0, 0, 0, 280);
    }

    private void editor_Unfocused(object sender, FocusEventArgs e)
    {
        KeyboardPadding.Margin = new Thickness(0,0, 0, 0);
    }
}