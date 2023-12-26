using Calendar.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Calendar.ViewModels;

public partial class ToDoTaskListViewModel : BaseViewModel
{
    private DatabaseContext databaseContext;

    private object syncToDoTasks;

    /// <summary>
    /// Gets or sets appointments.
    /// </summary>
    /// 
    [ObservableProperty]
    ObservableCollection<ToDoTaskViewModel> toDoTasks;

    [ObservableProperty]
    string newTaskDescription;

    private bool hasLoaded = false;

    public ToDoTaskListViewModel()
    {
        this.syncToDoTasks = new object();
        this.newTaskDescription = string.Empty;
        this.databaseContext = ServiceHelper.GetService<DatabaseContext>();
        this.toDoTasks = new ObservableCollection<ToDoTaskViewModel>();

        WeakReferenceMessenger.Default.Register<ToDoTaskInsertOrUpdateMessage>(this, async (r, m) =>
        {
            if (m != null && m.Value != null)
            {
                await SaveToDoTaskAsync(m.Value as ToDoTaskViewModel).ConfigureAwait(false);
            }
        });

        WeakReferenceMessenger.Default.Register<ToDoTaskCompleteMessage>(this, async (r, m) =>
        {
            if (m != null && m.Value != null)
            {
                var itemIndex = FindTaskIndex((int)m.Value);
                if (itemIndex >= 0)
                {
                    var operatingTask = ToDoTasks[itemIndex];
                    operatingTask.IsFinished = true;
                    await databaseContext.UpdateItemAsync<ToDoTask>(operatingTask.ToToDoTask()).ConfigureAwait(false);
                }
            }
        });
    }

    public async Task LoadToDoTasksAsync()
    {
        if (!hasLoaded)
        {
            hasLoaded = true;
            await GetAllToDoTasksAsync(replace:false);
            await EnsureOrderAsync();
        }
    }

    public async Task GetAllToDoTasksAsync(bool replace)
    {
        var table = await databaseContext.GetAllAsync<ToDoTask>();
        var toDoTaskRecords = table.OrderBy(item => item.Order).ToList();

        lock (syncToDoTasks)
        {
            ToDoTasks ??= new ObservableCollection<ToDoTaskViewModel>();
            if (replace)
            {
                ToDoTasks.Clear();
            }
            if (toDoTaskRecords is not null && toDoTaskRecords.Any())
            {
                foreach (var theTask in toDoTaskRecords)
                {
                    ToDoTasks.Add(new ToDoTaskViewModel(theTask));
                }
            }
        }
    }

    [RelayCommand]
    public void Erase()
    {
        this.newTaskDescription = string.Empty;
    }

    [RelayCommand]
    public async Task Add()
    {
        if (string.IsNullOrEmpty(this.NewTaskDescription))
        {
            return;
        }

        ToDoTaskViewModel toDoTask = new ToDoTaskViewModel
        {
            Order = this.ToDoTasks.Count + 1,
            Description = this.NewTaskDescription.Trim()
        };

        await SaveToDoTaskAsync(toDoTask).ConfigureAwait(false);

        this.NewTaskDescription = string.Empty;
    }

    [RelayCommand]
    private async Task Check(Object obj)
    {
        var thisToDoTask = (obj as ToDoTaskViewModel);
        if (thisToDoTask == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            thisToDoTask.IsFinished = !thisToDoTask.IsFinished;
            await databaseContext.UpdateItemAsync<ToDoTask>(thisToDoTask.ToToDoTask());
        }
    }

    [RelayCommand]
    public async Task EditToDoTask(Object obj)
    {
        var thisToDoTask = (obj as ToDoTaskViewModel);
        if (thisToDoTask == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            var navigationParameter = new Dictionary<string, object>
            {
                {"OperatingTask", thisToDoTask}
            };

            await Shell.Current.GoToAsync($"//MainPage/EditToDoTaskPage", navigationParameter);
        }
    }

    [RelayCommand]
    public async Task DeleteToDoTask(Object obj)
    {
        var thisToDoTask = (obj as ToDoTaskViewModel);
        if (thisToDoTask != null)
        {
            await ExecuteAsync(async () =>
            {
                var id = thisToDoTask.Id;
                if (await databaseContext.DeleteItemByKeyAsync<ToDoTask>(id))
                {
                    lock (this.syncToDoTasks)
                    {
                        ToDoTasks.Remove(thisToDoTask);
                    }

                    await EnsureOrderAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "ToDoTask was not deleted", "Ok");
                }
            });
        }
        else
        {
            await Task.CompletedTask;
        }
    }


    [RelayCommand]
    private async Task LaunchTimer(Object obj)
    {
        var thisToDoTask = obj as ToDoTaskViewModel;
        if (thisToDoTask == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            var navigationParameter = new Dictionary<string, object>
            {
                {"TaskType", TaskType.ToDoTask },
                {"Id", thisToDoTask.Id},
                {"TaskDescription", thisToDoTask.Description},
                {"SelectedHour", 0 },
                {"SelectedMinute", 30 },
                {"SelectedSecon", 0 }
            };

            await Shell.Current.GoToAsync($"//MainPage/TaskTimerPage", navigationParameter);
        }
    }

    public async Task OnItemDragging(Syncfusion.Maui.ListView.ItemDraggingEventArgs e)
    {
        //if (e.Action == Syncfusion.Maui.ListView.DragAction.Start)
        //{
        //    this.NewTaskDescription = string.Format("New Index {0}: {1}, Old Index {2}:{3}\n", e.NewIndex, this.ToDoTasks[e.NewIndex].Description, e.OldIndex, this.ToDoTasks[e.OldIndex].Description);
        //    for (int i = 0; i < this.ToDoTasks.Count; i++)
        //    {
        //        this.NewTaskDescription = this.NewTaskDescription + string.Format("{0}:{2}, {1}\n", i, this.ToDoTasks[i].Description, this.ToDoTasks[i].Order);
        //    }

        //}
        if (e.Action == Syncfusion.Maui.ListView.DragAction.Drop)
        {
            //this.NewTaskDescription = string.Format("New Index {0}: {1}, Old Index {2}:{3}\n", e.NewIndex, this.ToDoTasks[e.NewIndex].Description, e.OldIndex, this.ToDoTasks[e.OldIndex].Description);
            //for (int i = 0; i < this.ToDoTasks.Count; i++)
            //{
            //    this.NewTaskDescription = this.NewTaskDescription + string.Format("{0}:{2}, {1}\n", i, this.ToDoTasks[i].Description, this.ToDoTasks[i].Order);
            //}
            if (e.NewIndex != e.OldIndex)
            {
                var old = this.ToDoTasks[e.OldIndex];
                List<ToDoTaskViewModel> toSave = new List<ToDoTaskViewModel>(Math.Abs(e.NewIndex - e.OldIndex) + 1);
                
                old.Order = e.NewIndex + 1;
                toSave.Add(old);

                if (e.NewIndex < e.OldIndex)
                {
                    for (int i = e.OldIndex - 1; i >= e.NewIndex; i--)
                    {
                        var task = this.ToDoTasks[i];
                        task.Order++;
                        toSave.Add(task);
                    }
                }
                else
                {
                    for (int i = e.OldIndex + 1; i <= e.NewIndex; i++)
                    {
                        var task = this.ToDoTasks[i];
                        task.Order--;
                        toSave.Add(task);
                    }
                }

                foreach (var task in toSave) 
                {
                    await databaseContext.UpdateItemAsync<ToDoTask>(task.ToToDoTask());
                }
            }

            //this.NewTaskDescription = this.NewTaskDescription + "--- After ---\n";
            //for (int i = 0; i < this.ToDoTasks.Count; i++)
            //{
            //    this.NewTaskDescription = this.NewTaskDescription + string.Format("{0}:{2}, {1}\n", i, this.ToDoTasks[i].Description, this.ToDoTasks[i].Order);
            //}
        }
        else
        {
            await Task.CompletedTask;
        }       
    }

    private void InsertToDoTask(ToDoTaskViewModel theToDoTask)
    {
        int toDoTaskCount = this.ToDoTasks.Count;
        int lo = 0;
        int hi = toDoTaskCount - 1;

        while (lo < hi)
        {
            int m = (hi + lo) / 2;  // this might overflow; be careful.
            if (this.ToDoTasks[m].Order < theToDoTask.Order)
            {
                lo = m + 1;
            }
            else
            {
                hi = m - 1;
            }
        }

        if (toDoTaskCount > 0)
        {
            if (this.ToDoTasks[lo].Order < theToDoTask.Order)
            {
                lo++;
            }
        }

        ToDoTasks.Insert(lo, theToDoTask);
    }

    public async Task EnsureOrderAsync()
    {
        int taskCount = this.ToDoTasks.Count;
        for (int i = 0; i < taskCount; i++)
        {
            var task = this.ToDoTasks[i];
            if (task.Order != i + 1)
            {
                task.Order = i + 1;
                await databaseContext.UpdateItemAsync<ToDoTask>(task.ToToDoTask());
            }
        }
    }

    private async Task SaveToDoTaskAsync(ToDoTaskViewModel operatingTask)
    {
        if (operatingTask.Id == 0)
        {
            // Create ToDoTask
            var dbToDoTask = operatingTask.ToToDoTask();
            var result = await databaseContext.AddItemAsync<ToDoTask>(dbToDoTask);
            if (result)
            {
                operatingTask.Id = dbToDoTask.Id;

                InsertToDoTask(operatingTask);
            }
        }
        else
        {
            // Update event
            if (await databaseContext.UpdateItemAsync<ToDoTask>(operatingTask.ToToDoTask()))
            {
                lock (syncToDoTasks)
                {
                    var updateTaskIndex = FindTaskIndex(operatingTask.Id);
                    if (updateTaskIndex >= 0)
                    {
                        ToDoTasks[updateTaskIndex] = operatingTask;
                    }
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "ToDoTask updation error", "Ok");
            }
        }
    }

    private int FindTaskIndex(int taskId)
    {
        int found = -1;
        for (int i = 0; i < ToDoTasks.Count; ++i)
        {
            if (ToDoTasks[i].Id == taskId)
            {
                found = i;
                break;
            }
        }

        return found;
    }
    private async Task ExecuteAsync(Func<Task> operation)
    {
        try
        {
            await operation?.Invoke();
        }
        catch (Exception)
        {
        }
        finally
        {
        }
    }
}
