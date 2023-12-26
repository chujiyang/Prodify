using Calendar.ViewModels;

namespace Calendar.Pages;

public partial class MainPage : BasePage<MainViewModel>
{
    public MainPage(MainViewModel vm) : base(vm)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        (MyEventListView as Calendar.Views.EventListView)?.OnAppearing();
        (MyTodoListView as Calendar.Views.ToDoTaskListView)?.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        (MyEventListView as Calendar.Views.EventListView)?.OnDisappearing();
        (MyTodoListView as Calendar.Views.ToDoTaskListView)?.OnDisappearing();
    }
}

