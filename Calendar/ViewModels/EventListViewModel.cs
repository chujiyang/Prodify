using Calendar.Data;
using Calendar.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.LocalNotification;
using System.Collections.ObjectModel;

namespace Calendar.ViewModels;

public partial class EventListViewModel : BaseViewModel
{
    private DatabaseContext databaseContext;

    private List<ExceptionEvent> exceptionEvents;

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

    private int DaysInTheFuture
    {
        get { return 90; }
    }

    private int DaysInThePast
    {
        get { return 90; }
    }

    private EventViewModel OperatingEvent {  get; set; }

    public EventListViewModel()
    {
        syncEvents = new object();
        databaseContext = ServiceHelper.GetService<DatabaseContext>();

        events = new ObservableCollection<EventViewModel>();
        exceptionEvents = new List<ExceptionEvent>();
        displayDate = DateTime.Now.Date.AddHours(8).AddMinutes(50);
        minDateTime = DateTime.Now.Date.AddMonths(-3);
        maxDateTime = DateTime.Now.AddMonths(3);

        WeakReferenceMessenger.Default.Register<EventInsertOrUpdateMessage>(this, async (r, m) =>
        {
            if (m != null && m.Value != null)
            {
                var thisEvent = m.Value as EventViewModel;
                if (thisEvent != null)
                {
                    await SaveEventAsync(m.Value as EventViewModel, m.IsEditingSeries).ConfigureAwait(false);
                }
            }
            else
            {
                await Task.CompletedTask;
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
                    theEvent.FinishedTime = DateTime.Now;
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
            var firstDay = today.AddDays(-DaysInThePast);
            var lastDay = today.AddDays(DaysInTheFuture);

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
            {"DialogTitle", "Create Task" },
            {"IsEditingSeries", true }
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
        OperatingEvent = (obj as EventViewModel);
        if (OperatingEvent == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            OperatingEvent.IsFinished = !OperatingEvent.IsFinished;
            if (OperatingEvent.IsFinished)
            {
                OperatingEvent.FinishedTime = DateTime.Now;
            }
            else
            {
                OperatingEvent.FinishedTime = null;
            }

            await UpdateInstanceStatus(OperatingEvent);

            OperatingEvent = null;
        }
    }

    [RelayCommand]
    private async Task LaunchTimer(Object obj)
    {
        OperatingEvent = obj as EventViewModel;
        if (OperatingEvent == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            var timeSpan = OperatingEvent.To - OperatingEvent.From;

            var navigationParameter = new Dictionary<string, object>
            {
                {"TaskType", TaskType.Event},
                {"Id", OperatingEvent.Id},
                {"TaskDescription", OperatingEvent.EventName},
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
        OperatingEvent = (obj as EventViewModel);
        if (OperatingEvent == null)
        {
            await Task.CompletedTask;
        }
        else
        {            
            var navigationParameter = new Dictionary<string, object>
            {
                {"OperatingEvent", new EventViewModel(OperatingEvent)},
                {"DialogTitle", "Update Task" },
                {"IsEditingSeries", false }
            };

            await Shell.Current.GoToAsync($"//MainPage/EventDetailPage", navigationParameter);
        }
    }

    [RelayCommand]
    private async Task EditEventSeries(Object obj)
    {
        OperatingEvent = (obj as EventViewModel);
        if (OperatingEvent == null)
        {
            await Task.CompletedTask;
        }
        else
        {
            var navigationParameter = new Dictionary<string, object>
            {
                {"OperatingEvent", new EventViewModel(OperatingEvent)},
                {"DialogTitle", "Update Task" },
                {"IsEditingSeries", OperatingEvent.IsRecurring }
            };

            await Shell.Current.GoToAsync($"//MainPage/EventDetailPage", navigationParameter);
        }
    }

    [RelayCommand]
    private async Task DeleteEvent(Object obj)
    {
        OperatingEvent = (obj as EventViewModel);
        if (OperatingEvent != null)
        {
            if (OperatingEvent.IsRecurring)
            {
                /// BUBBUG how to deal with the alert of exception
                /// 
                // Add exception event
                ExceptionEvent exceptionEvent = new ExceptionEvent { EventId = OperatingEvent.Id, ExceptionTime = OperatingEvent.Date.ChangeTime(OperatingEvent.From) };
                AddExceptionEvent(exceptionEvent);

                Events.Remove(OperatingEvent);
            }
            else
            {
                RemoveAlert(OperatingEvent);

                await ExecuteAsync(async () =>
                {
                    var id = OperatingEvent.Id;
                    if (await databaseContext.DeleteItemByKeyAsync<Event>(id))
                    {
                        lock (syncEvents)
                        {
                            RemoveAllEvents(id);
                        }
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Delete Error", "Event was not deleted", "Ok");
                    }
                });
            }

            OperatingEvent = null;
        }
        else
        {
            await Task.CompletedTask;
        }
    }

    [RelayCommand]
    private async Task DeleteEventSeries(Object obj)
    {
        OperatingEvent = (obj as EventViewModel);
        if (OperatingEvent != null)
        {
            RemoveAlert(OperatingEvent);

            await ExecuteAsync(async () =>
            {
                var id = OperatingEvent.Id;
                if (await databaseContext.DeleteItemByKeyAsync<Event>(id))
                {
                    lock (syncEvents)
                    {
                        RemoveAllEvents(id);                        
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "Event was not deleted", "Ok");
                }

                if (OperatingEvent.IsRecurring)
                {
                    RemoveExceptionEvents(id);
                }
            });

            OperatingEvent = null;
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

    private void AddAlert(EventViewModel operatingEvent)
    {
        if (operatingEvent.AlertType == AlertType.NoAlert)
        {
            return;
        }

        if (operatingEvent.RecurrenceFrequencyId == 0)
        {
            var notificationTime = operatingEvent.Date + operatingEvent.From;

            var notification = new NotificationRequest
            {
                NotificationId = operatingEvent.Id,
                CategoryType = operatingEvent.AlertType == AlertType.Alarm ? NotificationCategoryType.Alarm : NotificationCategoryType.Reminder,
                Title = operatingEvent.EventName,
                Description = operatingEvent.Notes,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notificationTime,
                    RepeatType = NotificationRepeat.No
                }
            };
            LocalNotificationCenter.Current.Show(notification);
        }
        else
        {
            var notificationTime = CalculateNextOccuranceDate(operatingEvent).ChangeTime(operatingEvent.From);

            for (int i = 0; i < 7; i++)
            {
                if ((operatingEvent.RecurrencePattern & (1 << i)) != 0)
                {
                    var notification = new NotificationRequest
                    {
                        NotificationId = operatingEvent.Id + (1 << (24 + i)),
                        CategoryType = operatingEvent.AlertType == AlertType.Alarm ? NotificationCategoryType.Alarm : NotificationCategoryType.Reminder,
                        Title = operatingEvent.EventName,
                        Description = operatingEvent.Notes,
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = notificationTime.AddDays(i),
                            RepeatType = NotificationRepeat.Weekly
                        }
                    };
                    LocalNotificationCenter.Current.Show(notification);
                }
            }
        }
    }

    private void RemoveAlert(EventViewModel operatingEvent)
    {
        try
        {
            var notificationIdList = new List<int>();
            if (operatingEvent.RecurrenceFrequencyId == 0)
            {
                notificationIdList.Add(operatingEvent.Id + (1 << 24));
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    if ((operatingEvent.RecurrencePattern & (1 << i)) != 0)
                    {
                        notificationIdList.Add(operatingEvent.Id + (1 << 24));
                    }
                }
            }

            LocalNotificationCenter.Current.Clear(notificationIdList.ToArray());
        }
        catch(Exception)
        {
        }
    }

    private async Task SaveEventAsync(EventViewModel operatingEvent, bool isEditingSerires)
    {
        try
        {
            if (operatingEvent.Id == 0)
            {
                operatingEvent.CreatedTime = DateTime.Now;

                // Create event
                var dbEvent = operatingEvent.ToEvent(); 
                if (operatingEvent.IsRecurring)
                {
                    dbEvent.To = dbEvent.To.AddYears(1);
                }

                var result = await databaseContext.AddItemAsync<Event>(dbEvent);
                if (result)
                {
                    operatingEvent.Id = dbEvent.Id;
                    AddAlert(operatingEvent);
                    if (operatingEvent.IsRecurring)
                    {
                        InsertEvent(operatingEvent, operatingEvent.StartTime.FirstSecondOfDate(), DateTime.Today.AddDays(DaysInTheFuture));
                    }
                    else
                    {
                        InsertSingleEvent(operatingEvent);
                    }
                }
            }
            else
            {
                if (OperatingEvent == null || operatingEvent.Id != OperatingEvent.Id)
                {
                    throw new Exception("operating event id changed.");
                }

                if (isEditingSerires)
                {
                    UpdateSeries(operatingEvent, OperatingEvent);
                }
                else
                {
                    UpdateInstance(operatingEvent, OperatingEvent);
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private async Task UpdateInstanceStatus(EventViewModel updatedOperatingEvent)
    {
        if (updatedOperatingEvent.IsRecurring)
        {
            // Add exception event
            ExceptionEvent exceptionEvent = new ExceptionEvent { EventId = updatedOperatingEvent.Id, ExceptionTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.From) };
            AddExceptionEvent(exceptionEvent);

            EventViewModel newInstance = new EventViewModel(updatedOperatingEvent);
            newInstance.RecurrenceFrequencyId = 0;
            newInstance.RecurrenceInterval = 0;
            newInstance.IsRecurring = false;
            newInstance.RecurrencePattern = 0;
            newInstance.Id = 0;
            newInstance.LinkedId = updatedOperatingEvent.Id;
            newInstance.IsRecurring = false;
            newInstance.StartTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.From);
            newInstance.EndTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.To);

            await SaveEventAsync(newInstance, false);
        }
        else
        {
            await databaseContext.UpdateItemAsync<Event>(updatedOperatingEvent.ToEvent());
        }
    }

    private async Task UpdateInstance(EventViewModel updatedOperatingEvent, EventViewModel oldEvent)
    {
        if (oldEvent.AlertType != AlertType.NoAlert)
        {
            RemoveAlert(oldEvent);
        }

        if (updatedOperatingEvent.IsRecurring)
        {
            // Add exception event
            ExceptionEvent exceptionEvent = new ExceptionEvent { EventId = updatedOperatingEvent.Id, ExceptionTime = oldEvent.Date.ChangeTime(oldEvent.From) };
            AddExceptionEvent(exceptionEvent);

            EventViewModel newInstance = new EventViewModel(updatedOperatingEvent);
            newInstance.RecurrenceFrequencyId = 0;
            newInstance.RecurrenceInterval = 0;
            newInstance.IsRecurring = false;
            newInstance.RecurrencePattern = 0;
            newInstance.Id = 0;
            newInstance.LinkedId = updatedOperatingEvent.Id;
            newInstance.StartTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.From);
            newInstance.EndTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.To);
            Events.Remove(oldEvent);

            await SaveEventAsync(newInstance, false);
        }
        else
        {
            if (updatedOperatingEvent.AlertType != AlertType.NoAlert)
            {
                AddAlert(updatedOperatingEvent);
            }

            Events.Remove(oldEvent);

            await databaseContext.UpdateItemAsync<Event>(updatedOperatingEvent.ToEvent());

            InsertSingleEvent(updatedOperatingEvent);
        }

        OperatingEvent = null;
    }

    private async Task UpdateSeries(EventViewModel updatedOperatingEvent, EventViewModel oldEvent)
    {
        if (oldEvent.AlertType != AlertType.NoAlert)
        {
            RemoveAlert(oldEvent);
        }

        if (oldEvent.IsRecurring)
        {
            var lastEvent = LastEventInstanceBefore(oldEvent.Id, DateTime.Now);
            RemoveAllEventsAfter(oldEvent.Id, DateTime.Now);

            // Update the old events
            if (lastEvent != null)
            {
                // update the to time
                var lastRecurringEvent = new EventViewModel(lastEvent);
                lastRecurringEvent.EndTime = lastEvent.Date.ChangeTime(lastEvent.To);
                lastRecurringEvent.Date = lastEvent.Date;
                await databaseContext.UpdateItemAsync<Event>(lastRecurringEvent.ToEvent());
            }
            else
            {
                await databaseContext.DeleteItemByKeyAsync<Event>(oldEvent.Id);
            }

            updatedOperatingEvent.Id = 0;
            updatedOperatingEvent.StartTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.From);
            updatedOperatingEvent.EndTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.To); // SaveEventAsync will add years

            await SaveEventAsync(updatedOperatingEvent, true);
            return;
        }
        else
        {
            Events.Remove(oldEvent);
        }

        if (updatedOperatingEvent.AlertType != AlertType.NoAlert)
        {
            AddAlert(updatedOperatingEvent);
        }

        await databaseContext.UpdateItemAsync<Event>(updatedOperatingEvent.ToEvent());

        InsertEvent(updatedOperatingEvent, DateTime.Now, DateTime.Now.AddDays(DaysInTheFuture));

        OperatingEvent = null;
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
        GetExceptionEvents(start, end);

        var table = await databaseContext.GetTableAsync<Event>();
        var eventRecords = await table.Where(item => item.From < end && item.To >= start).OrderBy(item => item.From).ToListAsync();

        lock (syncEvents)
        {
            if (replace)
            {
                Events = new ObservableCollection<EventViewModel>();
            }
            if (eventRecords is not null && eventRecords.Any())
            {
                var events = new List<EventViewModel>();

                foreach (var theEvent in eventRecords)
                {
                    if (theEvent.RecurrenceFrequencyId != 0)
                    {
                        // Find the first occurence time
                        // Enumerate all patterns
                        var firstStart = theEvent.From.FirstDayOfWeek();
                        while (firstStart < end)
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                if (((1 << i) & theEvent.RecurrencePattern) != 0)
                                {
                                    var currentDate = firstStart.AddDays(i);
                                    if (currentDate <= end)
                                    {
                                        var currentTime = theEvent.From.ChangeDate(currentDate);
                                        var exceptionEvent = FindExceptionEvent(currentTime, theEvent.Id);
                                        if (exceptionEvent == null)
                                        {
                                            var newInstance = new EventViewModel(theEvent, currentTime);
                                            events.Add(newInstance);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            firstStart = firstStart.AddDays(7);
                        }
                    }
                    else
                    {
                        events.Add(new EventViewModel(theEvent));
                    }
                }

                events.Sort();

                Events = new ObservableCollection<EventViewModel>(events);
            }
        }
    }

    private async Task GetExceptionEvents(DateTime start, DateTime end)
    {
        var table = await databaseContext.GetTableAsync<ExceptionEvent>();
        this.exceptionEvents = await table.Where(item => item.ExceptionTime >= start && item.ExceptionTime < end).ToListAsync();
    }

    private ExceptionEvent FindExceptionEvent(DateTime time, int eventId)
    {
        foreach (var exceptionEvent in this.exceptionEvents)
        {
            if (exceptionEvent.ExceptionTime == time && exceptionEvent.EventId == eventId)
            {
                return exceptionEvent;
            }
        }

        return null;
    }

    private ExceptionEvent FindExceptionEventById(int id)
    {
        foreach (var exceptionEvent in this.exceptionEvents)
        {
            if (exceptionEvent.Id == id)
            {
                return exceptionEvent;
            }
        }

        return null;
    }

    private async Task AddExceptionEvent(ExceptionEvent exceptionEvent)
    {
        exceptionEvent.Id = 0;
        var result = await databaseContext.AddItemAsync<ExceptionEvent>(exceptionEvent);
        if (result)
        {
            this.exceptionEvents.Add(exceptionEvent);
        }
    }

    private async Task RemoveExceptionEvent(ExceptionEvent exceptionEvent)
    {
        await ExecuteAsync(async () =>
        {
            var id = exceptionEvent.Id;
            if (await databaseContext.DeleteItemByKeyAsync<ExceptionEvent>(id))
            {
                lock (syncEvents)
                {
                    this.exceptionEvents.Remove(exceptionEvent);
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Delete Error", "Event was not deleted", "Ok");
            }
        });
    }

    private async Task RemoveExceptionEvents(int eventId)
    {
        List<ExceptionEvent> exceptionEventsToRemove = new List<ExceptionEvent>();

        foreach (var exceptionEvent in this.exceptionEvents)
        {
            if (exceptionEvent.EventId == eventId)
            {
                exceptionEventsToRemove.Add(exceptionEvent);
                await databaseContext.DeleteItemByKeyAsync<ExceptionEvent>(exceptionEvent.Id);
            }
        }

        foreach (var exceptionEventToRemove in exceptionEventsToRemove)
        {
            this.exceptionEvents.Remove(exceptionEventToRemove);
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

    private void InsertEvent(EventViewModel theEvent, DateTime start, DateTime end)
    {
        // Find the first occurence time
        // Enumerate all patterns
        var firstStart = start.FirstDayOfWeek();
        while (firstStart < end)
        {
            for (int i = 0; i < 7; i++)
            {
                if (((1 << i) & theEvent.RecurrencePattern) != 0)
                {
                    var currentDate = firstStart.AddDays(i);
                    if (currentDate < start)
                    {
                        continue;
                    }

                    if (currentDate <= end)
                    {
                        var newInstance = new EventViewModel(theEvent);
                        newInstance.Date = currentDate;
                        InsertSingleEvent(newInstance);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            firstStart = firstStart.AddDays(7);
        }
    }

    private void InsertSingleEvent(EventViewModel theEvent)
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

    private void RemoveAllEvents(int id)
    {
        for (int i = Events.Count - 1; i >= 0; i--)
        {
            var current = Events[i];
            if (current.Id == id)
            {
                Events.RemoveAt(i);
            }
        }
    }

    private void RemoveAllEventsAfter(int id, DateTime now)
    {
        for (int i = Events.Count - 1; i >= 0; i--)
        {
            var current = Events[i];
            if (current.Id == id && current.Date >= now.Date)
            {
                Events.RemoveAt(i);
            }
        }
    }

    private EventViewModel LastEventInstanceBefore(int id, DateTime before)
    {
        EventViewModel lastEvent = null;
        for (int i = 0; i < Events.Count; i++)
        {
            var current = Events[i];
            if (current.Id == id && current.Date < before)
            {
                if (lastEvent == null || lastEvent.Date < current.Date)
                {
                    lastEvent = current;
                }
            }
        }

        return lastEvent;
    }

    private DateTime CalculateLastOccuranceDate(EventViewModel theEvent)
    {
        DateTime now = DateTime.Today;

        for (int i = 0; i < 7; i++)
        {
            if (((1 << (int)(now.DayOfWeek)) & theEvent.RecurrencePattern) != 0)
            {
                break;
            }
            now = now.Subtract(TimeSpan.FromDays(1));
        }

        return now;
    }

    private DateTime CalculateNextOccuranceDate(EventViewModel theEvent)
    {
        DateTime now = DateTime.Now;

        for (int i = 0; i < 7; i++)
        {
            if (((1 << (int)(now.DayOfWeek)) & theEvent.RecurrencePattern) != 0)
            {
                break;
            }
            now = now.Add(TimeSpan.FromDays(1));
        }

        return now;
    }
}
