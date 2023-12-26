using Calendar.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calendar.ViewModels;

public partial class TodoListViewModel : BaseViewModel
{
    private const int DefaultId = 0;

    private DatabaseContext databaseContext;

    private object syncEvents;

    [ObservableProperty]
    private string toDoList;

    public TodoListViewModel()
    {
        syncEvents = new object();
        databaseContext = ServiceHelper.GetService<DatabaseContext>();
        toDoList = string.Empty;
    }

    public async Task LoadTodoListAsync()
    {
        var table = await databaseContext.GetAllAsync<ToDoList>();
        var todoList = table.FirstOrDefault();
        if (todoList == null)
        {
            await databaseContext.AddItemAsync(new ToDoList());
        }
        else
        {
            lock (syncEvents)
            {
                ToDoList = todoList.Content;
            }
        }
    }

    public async Task SaveTodoListAsync()
    {
        ToDoList toDoList = new ToDoList { Id = DefaultId, Content = ToDoList };
        await databaseContext.UpdateItemAsync<ToDoList>(toDoList);
    }

    [RelayCommand]
    private void Erase()
    {
        this.ToDoList = string.Empty;
    }
}
