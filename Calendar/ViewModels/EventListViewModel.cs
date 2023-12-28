using Calendar.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.Telemetry;
using System.Collections.ObjectModel;

namespace Calendar.ViewModels;

public partial class EventListViewModel : BaseViewModel
{
    private DatabaseContext databaseContext;

    private object syncEvents;

    /// <summary>
    /// Gets or sets appointments.
    /// </summary>
    /// 
    [ObservableProperty]
    ObservableCollection<EventViewModel> events;

    /// <summary>
    /// Gets or sets the schedule display date.
    /// </summary>
    [ObservableProperty]
    DateTime displayDate;
    
    /// <summary>
    /// Gets or sets the schedule min date time.
    /// </summary>
    [ObservableProperty]
    DateTime minDateTime;

    /// <summary>
    /// Gets or sets the schedule max date time.
    /// </summary>
    [ObservableProperty]
    DateTime maxDateTime;

    private bool hasLoaded = false;

    public IListView ListView { get; set; }

    public string TodayValue
    {
        get
        {
            return DateTime.Today.ToString("MMM dd, yyyy, ddd");
        }
    }

    public EventListViewModel()
    {
        syncEvents = new object();
        databaseContext = ServiceHelper.GetService<DatabaseContext>();

        events = new ObservableCollection<EventViewModel>();
        displayDate = DateTime.Now.Date.AddHours(8).AddMinutes(50);
        minDateTime = DateTime.Now.Date.AddMonths(-3);
        maxDateTime = DateTime.Now.AddMonths(3);

        WeakReferenceMessenger.Default.Register<EventInsertOrUpdateMessage>(this, async (r, m) =>
        {
            if (m != null && m.Value != null)
            {
                await SaveEventAsync(m.Value as EventViewModel).ConfigureAwait(false);
            }
        });

        WeakReferenceMessenger.Default.Register<EventCompleteMessage>(this, async (r, m) =>
        {
            if (m != null)
            {
                var itemIndex = FindItemIndex((int)m.Value);
                if (itemIndex >= 0)
                {
                    var theEvent = Events[itemIndex];
                    theEvent.IsFinished = true;
                    await databaseContext.UpdateItemAsync<Event>(theEvent.ToEvent()).ConfigureAwait(false);
                }
            }
        });

    }

    public async Task LoadEventsAsync()
    {
        if (!hasLoaded)
        {
            hasLoaded = true;
            var today = DateTime.Now.Date;
            var firstDay = today.AddDays(-90);
            var lastDay = today.AddDays(366);

            await GetEventsAsync(firstDay, lastDay, true);

            TodayEvent();
        }
    }

    [RelayCommand]
    public async Task NewEvent()
    {
        var navigationParameter = new Dictionary<string, object>
        {
            {"OperatingEvent", new EventViewModel()},
            {"DialogTitle", "Create Task" }
        };

        await Shell.Current.GoToAsync($"//MainPage/EventDetailPage", navigationParameter);
    }

    [RelayCommand]
    public void TodayEvent()
    {
        if (this.ListView != null)
        {
            var todayIndex = FindFirstItemOfDate(DateTime.Today);
            if (todayIndex > 0 && todayIndex < this.Events.Count)
            {
                this.ListView.ScrollTo(this.Events[todayIndex], ScrollToPosition.MakeVisible);
            }
        }
    }

    [RelayCommand]
    private async Task CheckEvent(Object obj)
    {
        var thisEvent = (obj as EventViewModel);
        if (thisEvent == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            thisEvent.IsFinished = !thisEvent.IsFinished;
            await SaveEventAsync(thisEvent).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private async Task LaunchTimer(Object obj)
    {
        var thisEvent = obj as EventViewModel;
        if (thisEvent == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            var timeSpan = thisEvent.To - thisEvent.From;

            var navigationParameter = new Dictionary<string, object>
            {
                {"TaskType", TaskType.Event},
                {"Id", thisEvent.Id},
                {"TaskDescription", thisEvent.EventName},
                {"SelectedHour", timeSpan.Hours },
                {"SelectedMinute", timeSpan.Minutes },
                {"SelectedSecon", timeSpan.Seconds }
            };

            await Shell.Current.GoToAsync($"//MainPage/TaskTimerPage", navigationParameter);
        }
    }

    [RelayCommand]
    private async Task EditEvent(Object obj)
    {
        var thisEvent = (obj as EventViewModel);
        if (thisEvent == null)
        {
            await Task.CompletedTask;
        }
        else
        {            
            var navigationParameter = new Dictionary<string, object>
            {
                {"OperatingEvent", thisEvent},
                {"DialogTitle", "Update Task" }
            };

            await Shell.Current.GoToAsync($"//MainPage/EventDetailPage", navigationParameter);
        }
    }

    [RelayCommand]
    private async Task DeleteEvent(Object obj)
    {
        var thisEvent = (obj as EventViewModel);
        if (thisEvent != null)
        {
            await ExecuteAsync(async () =>
            {
                var id = thisEvent.Id;
                if (await databaseContext.DeleteItemByKeyAsync<Event>(id))
                {
                    lock (syncEvents)
                    {
                        Events.Remove(thisEvent);
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "Event was not deleted", "Ok");
                }
            });
        }
        else
        {
            await Task.CompletedTask;
        }
    }


    [RelayCommand]
    void ItemTapped(Object item)
    {
        var tappedItem = item as Syncfusion.Maui.ListView.ItemTappedEventArgs;
        if (tappedItem != null)
        {
            var tappedEvent = tappedItem.DataItem as EventViewModel;
            if (tappedEvent != null)
            {
                tappedEvent.IsNotesVisible = !tappedEvent.IsNotesVisible;
            }
        }
    }

    private async Task SaveEventAsync(EventViewModel operatingEvent)
    {
        if (operatingEvent.Id == 0)
        {
            // Create event
            var dbEvent = operatingEvent.ToEvent();
            var result = await databaseContext.AddItemAsync<Event>(dbEvent);
            if (result)
            {
                operatingEvent.Id = dbEvent.Id;

                InsertEvent(operatingEvent);
            }
        }
        else
        {
            // Update event
            if (await databaseContext.UpdateItemAsync<Event>(operatingEvent.ToEvent()))
            {
                lock (syncEvents)
                {
                    int removeIndex = -1;
                    for (int i = 0; i < Events.Count; ++i)
                    {
                        if (Events[i].Id == operatingEvent.Id)
                        {
                            removeIndex = i;
                            break;
                        }
                    }
                    if (removeIndex >= 0)
                    {
                        Events.RemoveAt(removeIndex);
                    }

                    InsertEvent(operatingEvent);
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Event updation error", "Ok");
            }
        }
    }

    private async Task ExecuteAsync(Func<Task> operation)
    {
        try
        {
            await operation?.Invoke();
        }
        catch (Exception)
        {
            /*
             * {System.TypeInitializationException: The type initializer for 'SQLite.SQLiteConnection' threw an exception.
             ---> System.IO.FileNotFoundException: Could not load file or assembly 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b' or one of its dependencies.
            File name: 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b'
               at SQLitePCL.Batteries_V2.Init()
               at SQLite.SQLiteConnection..cctor()
               --- End of inner exception stack trace ---
               at SQLite.SQLiteConnectionWithLock..ctor(SQLiteConnectionString connectionString)
               at SQLite.SQLiteConnectionPool.Entry..ctor(SQLiteConnectionString connectionString)
               at SQLite.SQLiteConnectionPool.GetConnectionAndTransactionLock(SQLiteConnectionString connectionString, Object& transactionLock)
               at SQLite.SQLiteConnectionPool.GetConnection(SQLiteConnectionString connectionString)
               at SQLite.SQLiteAsyncConnection.GetConnection()
               at SQLite.SQLiteAsyncConnection.<>c__DisplayClass33_0`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].<WriteAsync>b__0()
               at System.Threading.Tasks.Task`1[[SQLite.CreateTableResult, SQLite-net, Version=1.8.116.0, Culture=neutral, PublicKeyToken=null]].InnerInvoke()
               at System.Threading.Tasks.Task.<>c.<.cctor>b__273_0(Object obj)
               at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
            --- End of stack trace from previous location ---
               at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
               at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
            --- End of stack trace from previous location ---
               at MAUISql.Data.DatabaseContext.<CreateTableIfNotExists>d__6`1[[MAUISql.Models.Event, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 18
               at MAUISql.Data.DatabaseContext.<GetTableAsync>d__7`1[[MAUISql.Models.Event, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 23
               at MAUISql.Data.DatabaseContext.<GetAllAsync>d__8`1[[MAUISql.Models.Event, MAUISql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].MoveNext() in D:\MAUI\MAUISql\MAUISql\Data\DatabaseContext.cs:line 29
               at MAUISql.ViewModels.EventsViewModel.<LoadEventsAsync>b__6_0() in D:\MAUI\MAUISql\MAUISql\ViewModels\EventsViewModel.cs:line 34
               at MAUISql.ViewModels.EventsViewModel.ExecuteAsync(Func`1 operation, String busyText) in D:\MAUI\MAUISql\MAUISql\ViewModels\EventsViewModel.cs:line 103}
             */
        }
        finally
        {
        }
    }

    public async Task GetEventsAsync(DateTime start, DateTime end, bool replace)
    {
        var table = await databaseContext.GetTableAsync<Event>();
        var eventRecords = await table.Where(item => item.From >= start && item.From < end).OrderBy(item => item.From).ToListAsync();

        lock (syncEvents)
        {
            Events ??= new ObservableCollection<EventViewModel>();
            if (replace)
            {
                Events.Clear();
            }
            if (eventRecords is not null && eventRecords.Any())
            {
                foreach (var theEvent in eventRecords)
                {
                    Events.Add(new EventViewModel(theEvent));
                }
            }
        }
    }


    //public async Task GetTodayEventsAsync()
    //{
    //    var today = DateTime.Now.Date;
    //    var tomorrow = today.AddDays(1);

    //    await GetEventsAsync(today, tomorrow);
    //}

    //public async Task GetWeekEventsAsync()
    //{
    //    var today = DateTime.Now.Date;

    //    var sunday = today.CurrentWeek(DayOfWeek.Sunday);
    //    var nextSunday = today.NextWeek(DayOfWeek.Sunday);

    //    await GetEventsAsync(sunday, nextSunday);
    //}

    private void InsertEvent(EventViewModel theEvent)
    {
        int eventCount = this.Events.Count;
        int lo = 0;
        int hi = eventCount - 1;

        while (lo < hi)
        {
            int m = (hi + lo) / 2;  // this might overflow; be careful.
            if (this.Events[m].Date < theEvent.Date || (this.Events[m].Date == theEvent.Date && this.Events[m].From < theEvent.From))
            {
                lo = m + 1;
            }
            else
            {
                hi = m - 1;
            }
        }

        if (eventCount > 0)
        {
            if (this.Events[lo].Date < theEvent.Date || (this.Events[lo].Date == theEvent.Date && this.Events[lo].From < theEvent.From))
            {
                lo++;
            }
        }

        Events.Insert(lo, theEvent);
    }

    private int FindFirstItemOfDate(DateTime targetDate)
    {
        var eventCount = this.Events.Count;
        int lo = 0;
        int hi = eventCount;

        while (lo < hi)
        {
            int m = (hi + lo) / 2;  // this might overflow; be careful.
            if (targetDate <= this.Events[m].Date)
            {
                hi = m;
            }
            else
            {
                lo = m + 1;
            }
        }

        if (lo < eventCount && this.Events[lo].Date < targetDate)
        {
            lo++;
        }

        return lo;
    }

    private int FindItemIndex(int id)
    {
        if (Events == null)
        {
            return -1;
        }

        int eventCount = Events.Count;
        for (int i = 0; i < eventCount; ++i)
        {
            if (Events[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }
}
