using Calendar.ViewModels;

namespace Calendar.Views;

public partial class ToDoTaskListView : ContentView
{
    public ToDoTaskListView()
	{
        InitializeComponent();
        this.Loaded += ToDoTaskListView_Loaded;
        this.Unloaded += ToDoTaskListView_Unloaded;
    }

    private async void ToDoTaskListView_Unloaded(object sender, EventArgs e)
    {
        var toDoListViewModel = this.BindingContext as ToDoTaskListViewModel;
        if (toDoListViewModel != null)
        {
            await toDoListViewModel.EnsureOrderAsync().ConfigureAwait(false);
        }
    }

    private async void ToDoTaskListView_Loaded(object sender, EventArgs e)
    {
        var toDoListViewModel = this.BindingContext as ToDoTaskListViewModel;
        if (toDoListViewModel != null)
        {
            toDoListViewModel.ListView = new ListViewAdaptor(this.listView);

            await toDoListViewModel.LoadToDoTasksAsync().ConfigureAwait(false);
        }
    }

    private async void listView_ItemDragging(object sender, Syncfusion.Maui.ListView.ItemDraggingEventArgs e)
    {
        var toDoListViewModel = this.BindingContext as ToDoTaskListViewModel;
        if (toDoListViewModel != null)
        {
            await toDoListViewModel.OnItemDraggingAsync(e).ConfigureAwait(false);
        }
    }

    private void editor_Focused(object sender, FocusEventArgs e)
    {
        KeyboardPadding.Margin = new Thickness(0, 0, 0, 280);
    }

    private void editor_Unfocused(object sender, FocusEventArgs e)
    {
        KeyboardPadding.Margin = new Thickness(0, 0, 0, 0);
    }
}