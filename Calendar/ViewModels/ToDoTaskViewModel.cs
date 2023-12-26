using Calendar.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Calendar.ViewModels;

public partial class ToDoTaskViewModel : ObservableObject
{
    [ObservableProperty]
    public int id = 0;

    [ObservableProperty]
    public int order;

    [ObservableProperty]
    public int priority = 1;

    [ObservableProperty]
    public string description;

    [ObservableProperty]
    public bool isFinished;

    public ToDoTaskViewModel()
    {
        this.id = 0;
        this.order = 0;
        this.priority = 1;
        this.description = string.Empty;
        this.isFinished = false;
    }

    public ToDoTaskViewModel(ToDoTask toDoTask)
    {
        this.id = toDoTask.Id;
        this.order = toDoTask.Order;
        this.priority = toDoTask.Priority;
        this.description = toDoTask.Description;
        this.isFinished = toDoTask.IsFinished;
    }

    public ToDoTaskViewModel(ToDoTaskViewModel other)
    {
        this.id = other.Id;
        this.order = other.Order;
        this.priority = other.Priority;
        this.description = other.Description;
        this.isFinished= other.IsFinished;
    }

    public ToDoTask ToToDoTask()
    {
        return new ToDoTask
        {
            Id = this.Id,
            Order = this.Order,
            Priority = this.Priority,
            Description = this.Description,
            IsFinished = this.IsFinished,
        };
    }
}
