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
    enum Operations
    {
        Insert,
        Update,
        Delete,
        LoadMoreBottom,
        LoadMoreTop,
        Today
    };

    private DatabaseContext databaseContext;

    private List<ExceptionEvent> exceptionEvents;
    private List<EventViewModel> allEvents;
    private int startViewIndex;
    private int endViewIndex; // exclusive
    private int maxDisplayItems;

    private object syncExceptionEvents;
    private object syncEvents;

    /// <summary>
    /// Gets or sets appointments.
    /// </summary>
    /// 
    [ObservableProperty]
    ObservableCollection<EventViewModel> events;

    [ObservableProperty]
    bool indicatorIsVisible;

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
        get { return 60; }
    }

    private int DaysInThePast
    {
        get { return 60; }
    }

    private EventViewModel OperatingEvent {  get; set; }

    public EventListViewModel()
    {
        syncExceptionEvents = new object();
        syncEvents = new object();
        databaseContext = ServiceHelper.GetService<DatabaseContext>();
        allEvents = new List<EventViewModel>();
        startViewIndex = 0;
        endViewIndex = 0;
        maxDisplayItems = 20;
        exceptionEvents = new List<ExceptionEvent>();
        events = new ObservableCollection<EventViewModel>();
        displayDate = DateTime.Today.FirstSecondOfDate();
        minDateTime = displayDate.AddDays(-DaysInThePast);
        maxDateTime = displayDate.AddDays(DaysInTheFuture);

        WeakReferenceMessenger.Default.Register<EventInsertOrUpdateMessage>(this, (r, m) =>
        {
            if (m != null && m.Value != null)
            {
                var thisEvent = m.Value as EventViewModel;
                if (thisEvent != null)
                {
                    Task.Run(async () =>
                    {
                        await SaveEventAsync(new EventViewModel(thisEvent), m.IsEditingSeries).ConfigureAwait(false);
                    });
                }
            }
        });

        WeakReferenceMessenger.Default.Register<EventCompleteMessage>(this, (r, m) =>
        {
            if (m != null)
            {
                var itemIndex = FindItemIndex((int)m.Value);
                if (itemIndex >= 0)
                {
                    var theEvent = this.allEvents[itemIndex];

                    Task.Run(async () =>
                    {
                        theEvent.IsFinished = true;
                        theEvent.FinishedTime = DateTime.Now;
                        await databaseContext.UpdateItemAsync<Event>(theEvent.ToEvent()).ConfigureAwait(false);
                    });
                }
            }
        });

    }

    public async Task LoadEventsAsync()
    {
        if (!this.hasLoaded)
        {
            await Task.Run(async () =>
            {
                this.hasLoaded = true;
                var today = DateTime.Today.FirstSecondOfDate();
                this.minDateTime = today.AddDays(-DaysInThePast);
                this.maxDateTime = today.AddDays(DaysInTheFuture);

                await GetEventsAsync(this.minDateTime, this.maxDateTime, true).ConfigureAwait(false);

                TodayEvent();
            }).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    public async Task NewEvent()
    {
        var navigationParameter = new Dictionary<string, object>
        {
            {"OperatingEvent", new EventViewModel()},
            {"SeriesViewModel", new SeriesViewModel{ IsEditingSeries = true, ShowEditSeries = false}}
        };

        await Shell.Current.GoToAsync($"//MainPage/EventDetailPage", navigationParameter).ConfigureAwait(false);
    }

    [RelayCommand]
    public void TodayEvent()
    {
        if (this.ListView != null)
        {
            AdjustViewWindow(DateTime.Today.FirstSecondOfDate(), Operations.Today);
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

            await UpdateInstanceStatusAsync(OperatingEvent).ConfigureAwait(false);

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

            await Shell.Current.GoToAsync($"//MainPage/TaskTimerPage", navigationParameter).ConfigureAwait(false);
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
                {"SeriesViewModel", new SeriesViewModel{ IsEditingSeries = false, ShowEditSeries = OperatingEvent.IsRecurring} }
            };

            await Shell.Current.GoToAsync($"//MainPage/EventDetailPage", navigationParameter).ConfigureAwait(false);
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
                await AddExceptionEventAsync(exceptionEvent).ConfigureAwait(false);

                this.allEvents.Remove(OperatingEvent);
            }
            else
            {
                RemoveAlert(OperatingEvent);

                var id = OperatingEvent.Id;
                if (await databaseContext.DeleteItemByKeyAsync<Event>(id).ConfigureAwait(false))
                {
                    RemoveAllEvents(id);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "Event was not deleted", "Ok").ConfigureAwait(false);
                }
            }

            AdjustViewWindow(OperatingEvent.StartTime, Operations.Delete);
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

            var id = OperatingEvent.Id;
            if (await databaseContext.DeleteItemByKeyAsync<Event>(id).ConfigureAwait(false))
            {
                RemoveAllEvents(id);
            }
            else
            {
                await Shell.Current.DisplayAlert("Delete Error", "Event was not deleted", "Ok").ConfigureAwait(false);
            }

            if (OperatingEvent.IsRecurring)
            {
                await RemoveExceptionEventsAsync(id).ConfigureAwait(false);
            }

            AdjustViewWindow(OperatingEvent.StartTime, Operations.Delete);
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
                tappedEvent.IsNotesVisible = (!tappedEvent.IsNotesVisible) && !string.IsNullOrWhiteSpace(tappedEvent.Notes);
            }
        }
    }

    public void LoadMoreItems()
    {
        if (this.allEvents.Count > this.maxDisplayItems)
        {
            AdjustViewWindow(DateTime.Now, Operations.LoadMoreBottom);
        }
    }

    public void LoadMoreFromTop()
    {
        if (this.allEvents.Count > this.maxDisplayItems && this.startViewIndex > 0)
        {
            AdjustViewWindow(DateTime.Now, Operations.LoadMoreTop);
        }
    }
    private async Task AddAlertAsync(EventViewModel operatingEvent)
    {
        if (operatingEvent.AlertType == AlertType.NoAlert)
        {
            return;
        }

        try
        {
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled().ConfigureAwait(false) == false)
            {
                bool allowed = await LocalNotificationCenter.Current.RequestNotificationPermission(new NotificationPermission()).ConfigureAwait(false);
                if (!allowed)
                {
                    return;
                }
            }
        }
        catch (Exception)
        {
            return;
        }

        if (operatingEvent.RecurrenceFrequencyId == 0)
        {
            var notificationTime = operatingEvent.Date.ChangeTime(operatingEvent.From);

            var notification = new NotificationRequest
            {
                NotificationId = operatingEvent.Id,

                CategoryType = NotificationCategoryType.Reminder,
                Title = operatingEvent.EventName,
                Description = operatingEvent.Notes,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notificationTime,
                    NotifyAutoCancelTime = DateTime.Now.AddSeconds(25),
                    RepeatType = NotificationRepeat.No
                }
            };

            if (operatingEvent.AlertType == AlertType.Alarm)
            {
                notification.CategoryType = NotificationCategoryType.Alarm;
                notification.Sound = "clock.aiff";
                notification.Silent = false;
                notification.iOS = new Plugin.LocalNotification.iOSOption.iOSOptions{ PlayForegroundSound = true };
            }

            await LocalNotificationCenter.Current.Show(notification).ConfigureAwait(false);
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
                        CategoryType = NotificationCategoryType.Reminder,
                        Title = operatingEvent.EventName,
                        Description = operatingEvent.Notes,
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = notificationTime.AddDays(i),
                            NotifyAutoCancelTime = DateTime.Now.AddSeconds(25),
                            RepeatType = NotificationRepeat.Weekly
                        }
                    };

                    if (operatingEvent.AlertType == AlertType.Alarm)
                    {
                        notification.CategoryType = NotificationCategoryType.Alarm;
                        notification.Sound = "clock.aiff";
                        notification.Silent = false;
                        notification.iOS = new Plugin.LocalNotification.iOSOption.iOSOptions{ PlayForegroundSound = true };
                    }

                    await LocalNotificationCenter.Current.Show(notification).ConfigureAwait(false);
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
                notificationIdList.Add(operatingEvent.Id);
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    if ((operatingEvent.RecurrencePattern & (1 << i)) != 0)
                    {
                        notificationIdList.Add(operatingEvent.Id + (1 << (24 + i)));
                    }
                }
            }

            LocalNotificationCenter.Current.Cancel(notificationIdList.ToArray());
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
                // Create event
                var dbEvent = operatingEvent.ToEvent(); 
                var result = await databaseContext.AddItemAsync<Event>(dbEvent).ConfigureAwait(false);
                if (result)
                {
                    operatingEvent.Id = dbEvent.Id;
                    await AddAlertAsync(operatingEvent).ConfigureAwait(false);
                    if (operatingEvent.IsRecurring)
                    {
                        InsertEvent(operatingEvent, operatingEvent.StartTime.FirstSecondOfDate(), DateTime.Today.AddDays(DaysInTheFuture));
                    }
                    else
                    {
                        InsertSingleEvent(operatingEvent);
                    }
                }

                AdjustViewWindow(operatingEvent.StartTime, Operations.Insert);
            }
            else
            {
                if (OperatingEvent == null || operatingEvent.Id != OperatingEvent.Id)
                {
                    throw new Exception("operating event id changed.");
                }

                if (isEditingSerires)
                {
                    await UpdateSeriesAsync(operatingEvent, OperatingEvent).ConfigureAwait(false);
                }
                else
                {
                    await UpdateInstanceAsync(operatingEvent, OperatingEvent).ConfigureAwait(false);
                }

                AdjustViewWindow(operatingEvent.StartTime, Operations.Update);
            }
        }
        catch (Exception)
        {
        }
    }

    private async Task UpdateInstanceStatusAsync(EventViewModel updatedOperatingEvent)
    {
        if (updatedOperatingEvent.IsRecurring)
        {
            // Add exception event
            ExceptionEvent exceptionEvent = new ExceptionEvent { EventId = updatedOperatingEvent.Id, ExceptionTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.From) };
            await AddExceptionEventAsync(exceptionEvent).ConfigureAwait(false);


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

            allEvents.Remove(updatedOperatingEvent);

            await SaveEventAsync(newInstance, false).ConfigureAwait(false);
        }
        else
        {
            await databaseContext.UpdateItemAsync<Event>(updatedOperatingEvent.ToEvent()).ConfigureAwait(false);
        }
    }

    private async Task UpdateInstanceAsync(EventViewModel updatedOperatingEvent, EventViewModel oldEvent)
    {
        if (oldEvent.AlertType != AlertType.NoAlert)
        {
            RemoveAlert(oldEvent);
        }

        if (updatedOperatingEvent.IsRecurring)
        {
            // Add exception event
            ExceptionEvent exceptionEvent = new ExceptionEvent { EventId = updatedOperatingEvent.Id, ExceptionTime = oldEvent.Date.ChangeTime(oldEvent.From) };
            await AddExceptionEventAsync(exceptionEvent).ConfigureAwait(false);

            EventViewModel newInstance = new EventViewModel(updatedOperatingEvent);
            newInstance.RecurrenceFrequencyId = 0;
            newInstance.RecurrenceInterval = 0;
            newInstance.IsRecurring = false;
            newInstance.RecurrencePattern = 0;
            newInstance.Id = 0;
            newInstance.LinkedId = updatedOperatingEvent.Id;
            newInstance.StartTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.From);
            newInstance.EndTime = updatedOperatingEvent.Date.ChangeTime(updatedOperatingEvent.To);
            allEvents.Remove(oldEvent);

            await SaveEventAsync(newInstance, false).ConfigureAwait(false);
        }
        else
        {
            if (updatedOperatingEvent.AlertType != AlertType.NoAlert)
            {
                await AddAlertAsync(updatedOperatingEvent).ConfigureAwait(false);
            }

            this.allEvents.Remove(oldEvent);

            await databaseContext.UpdateItemAsync<Event>(updatedOperatingEvent.ToEvent()).ConfigureAwait(false);

            InsertSingleEvent(updatedOperatingEvent);
        }

        OperatingEvent = null;
    }

    private void AdjustViewWindow(DateTime startTime, Operations operation)
    {
        switch (operation)
        {
            case Operations.Insert:
            case Operations.Update:
            case Operations.Delete:
                {
                    var firstItemIndex = FindFirstItemOfDate(startTime);

                    var lastItemIndex = Math.Min(firstItemIndex + maxDisplayItems - 1, this.allEvents.Count - 1);
                    if (lastItemIndex - firstItemIndex < maxDisplayItems)
                    {
                        this.startViewIndex = Math.Max(0, lastItemIndex + 1 - this.maxDisplayItems);
                        this.endViewIndex = lastItemIndex;
                    }
                    else
                    {
                        this.startViewIndex = firstItemIndex;
                        this.endViewIndex = lastItemIndex;
                    }
                }
                break;
            case Operations.Today:
                {
                    var firstViewItemIndex = FindFirstItemOfDate(startTime);
                    var firstItemIndex = firstViewItemIndex;
                    if (firstViewItemIndex > 0)
                    {
                        firstItemIndex = Math.Max(0, firstViewItemIndex - 3);
                    }

                    var lastItemIndex = Math.Min(firstItemIndex + maxDisplayItems - 1, this.allEvents.Count - 1);
                    if (lastItemIndex - firstItemIndex < maxDisplayItems)
                    {
                        this.startViewIndex = Math.Max(0, lastItemIndex + 1 - this.maxDisplayItems);
                        this.endViewIndex = lastItemIndex;
                    }
                    else
                    {
                        this.startViewIndex = firstItemIndex;
                        this.endViewIndex = lastItemIndex;
                    }
                }
                break;
            case Operations.LoadMoreTop:
                {
                    if (this.startViewIndex == 0)
                    {
                        return;
                    }
                    this.startViewIndex -= this.maxDisplayItems / 3;
                    if (this.startViewIndex < 0)
                    {
                        this.startViewIndex = 0;
                    }
                    this.endViewIndex = Math.Min(this.startViewIndex + this.maxDisplayItems - 1, this.allEvents.Count - 1);
                }
                break;
            case Operations.LoadMoreBottom:
                {
                    if (this.endViewIndex == this.allEvents.Count - 1)
                    {
                        return;
                    }
                    this.endViewIndex += this.maxDisplayItems / 3;
                    if (this.endViewIndex > this.allEvents.Count - 1)
                    {
                        this.endViewIndex = this.allEvents.Count - 1;
                    }
                    this.startViewIndex = this.endViewIndex - this.maxDisplayItems;
                    if (this.startViewIndex < 0)
                    {
                        this.startViewIndex = 0;
                    }
                }
                break;
        }

        var newEvents = new ObservableCollection<EventViewModel>();
        for (int i = this.startViewIndex; i <= this.endViewIndex; i++)
        {
            newEvents.Add(this.allEvents[i]);
        }

        OperatingEvent = null;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            this.ListView.BeginInit();
            this.Events = newEvents;
            this.ListView.EndInit();
        });
    }

    private async Task UpdateSeriesAsync(EventViewModel updatedOperatingEvent, EventViewModel oldEvent)
    {
        if (oldEvent.AlertType != AlertType.NoAlert)
        {
            RemoveAlert(oldEvent);
        }

        if (oldEvent.IsRecurring)
        {

            var lastEventEndTime = oldEvent.Date.ChangeTime(oldEvent.From);
            if (lastEventEndTime < DateTime.Now)
            {
                lastEventEndTime = DateTime.Now;
            }

            var lastEvent = LastEventInstanceBefore(oldEvent.Id, lastEventEndTime);
            RemoveAllEventsAfter(oldEvent.Id, lastEventEndTime);

            // Update the old events
            if (lastEvent != null)
            {
                // update the to time
                var lastRecurringEvent = new EventViewModel(lastEvent);
                lastRecurringEvent.EndTime = lastEvent.Date.ChangeTime(lastEvent.To);
                lastRecurringEvent.Date = lastEvent.Date;
                await databaseContext.UpdateItemAsync<Event>(lastRecurringEvent.ToEvent()).ConfigureAwait(false);
            }
            else
            {
                await databaseContext.DeleteItemByKeyAsync<Event>(oldEvent.Id).ConfigureAwait(false);
            }

            updatedOperatingEvent.Id = 0;

            await SaveEventAsync(updatedOperatingEvent, true).ConfigureAwait(false);
            return;
        }
        else
        {
            this.allEvents.Remove(oldEvent);
        }

        if (updatedOperatingEvent.AlertType != AlertType.NoAlert)
        {
            await AddAlertAsync(updatedOperatingEvent).ConfigureAwait(false);
        }

        await databaseContext.UpdateItemAsync<Event>(updatedOperatingEvent.ToEvent()).ConfigureAwait(false);

        InsertEvent(updatedOperatingEvent, DateTime.Now, DateTime.Now.AddDays(DaysInTheFuture));
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
        await GetExceptionEventsAsync(start, end).ConfigureAwait(false);

        var table = await databaseContext.GetTableAsync<Event>().ConfigureAwait(false);
        var eventRecords = await table.Where(item => item.From < end && item.To >= start).OrderBy(item => item.From).ToListAsync().ConfigureAwait(false);

        lock (syncEvents)
        {
            this.allEvents.Clear();

            if (eventRecords is not null && eventRecords.Any())
            {
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
                                var currentDate = firstStart.AddDays(i);
                                if (currentDate < theEvent.From.FirstSecondOfDate() || currentDate < start)
                                {
                                    continue;
                                }
                                if (currentDate > end)
                                {
                                    break;
                                }
                                if (((1 << i) & theEvent.RecurrencePattern) != 0)
                                {
                                    var currentTime = theEvent.From.ChangeDate(currentDate);
                                    var exceptionEvent = FindExceptionEvent(currentTime, theEvent.Id);
                                    if (exceptionEvent == null)
                                    {
                                        var newInstance = new EventViewModel(theEvent, currentTime);
                                        this.allEvents.Add(newInstance);
                                    }
                                }
                            }

                            firstStart = firstStart.AddDays(7);
                        }
                    }
                    else
                    {
                        this.allEvents.Add(new EventViewModel(theEvent));
                    }
                }

                this.allEvents.Sort();
            }
        }
    }

    private async Task GetExceptionEventsAsync(DateTime start, DateTime end)
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

    private async Task AddExceptionEventAsync(ExceptionEvent exceptionEvent)
    {
        exceptionEvent.Id = 0;
        var result = await databaseContext.AddItemAsync<ExceptionEvent>(exceptionEvent).ConfigureAwait(false);
        if (result)
        {
            this.exceptionEvents.Add(exceptionEvent);
        }
    }

    private async Task RemoveExceptionEventAsync(ExceptionEvent exceptionEvent)
    {
        var id = exceptionEvent.Id;
        if (await databaseContext.DeleteItemByKeyAsync<ExceptionEvent>(id).ConfigureAwait(false))
        {
            this.exceptionEvents.Remove(exceptionEvent);
        }
        else
        {
            await Shell.Current.DisplayAlert("Delete Error", "Event was not deleted", "Ok");
        }
    }

    private async Task RemoveExceptionEventsAsync(int eventId)
    {
        List<ExceptionEvent> exceptionEventsToRemove = new List<ExceptionEvent>();

        foreach (var exceptionEvent in this.exceptionEvents)
        {
            if (exceptionEvent.EventId == eventId)
            {
                exceptionEventsToRemove.Add(exceptionEvent);
                await databaseContext.DeleteItemByKeyAsync<ExceptionEvent>(exceptionEvent.Id).ConfigureAwait(false);
            }
        }

        foreach (var exceptionEventToRemove in exceptionEventsToRemove)
        {
            this.exceptionEvents.Remove(exceptionEventToRemove);
        }
    }

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
        int eventCount = this.allEvents.Count;
        int lo = 0;
        int hi = eventCount - 1;

        while (lo < hi)
        {
            int m = (hi + lo) / 2;  // this might overflow; be careful.
            if (this.allEvents[m].Date < theEvent.Date || (this.allEvents[m].Date == theEvent.Date && this.allEvents[m].From < theEvent.From))
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
            if (this.allEvents[lo].Date < theEvent.Date || (this.allEvents[lo].Date == theEvent.Date && this.allEvents[lo].From < theEvent.From))
            {
                lo++;
            }
        }

        this.allEvents.Insert(lo, theEvent);
    }

    private int FindFirstViewItemOfDate(DateTime targetDate)
    {
        var eventCount = this.Events.Count;
        int lo = 0;
        int hi = eventCount;

        while (lo < hi)
        {
            int m = (hi + lo) / 2;  // this might overflow; be careful.
            if (targetDate <= this.Events[m].Date + this.Events[m].From)
            {
                hi = m;
            }
            else
            {
                lo = m + 1;
            }
        }

        if (lo == eventCount)
        {
            lo = eventCount - 1;
        }
        if (lo < 0)
        {
            lo = 0;
        }

        return lo;
    }

    private int FindFirstItemOfDate(DateTime targetDate)
    {
        var eventCount = this.allEvents.Count;
        int lo = 0;
        int hi = eventCount;

        while (lo < hi)
        {
            int m = (hi + lo) / 2;  // this might overflow; be careful.
            if (targetDate <= this.allEvents[m].Date + this.allEvents[m].From)
            {
                hi = m;
            }
            else
            {
                lo = m + 1;
            }
        }

        if (lo == eventCount)
        {
            lo = eventCount - 1;
        }
        if (lo < 0)
        {
            lo = 0;
        }

        //if (lo < eventCount && this.allEvents[lo].Date + this.allEvents[lo].From < targetDate)
        //{
        //    lo++;
        //}

        return lo;
    }

    private int FindItemIndex(int id)
    {
        int eventCount = this.allEvents.Count;
        for (int i = 0; i < eventCount; ++i)
        {
            if (this.allEvents[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    private void RemoveAllEvents(int id)
    {
        for (int i = this.allEvents.Count - 1; i >= 0; i--)
        {
            var current = this.allEvents[i];
            if (current.Id == id)
            {
                this.allEvents.RemoveAt(i);
            }
        }
    }

    private void RemoveAllEventsAfter(int id, DateTime now)
    {
        for (int i = this.allEvents.Count - 1; i >= 0; i--)
        {
            var current = this.allEvents[i];
            if (current.Id == id && current.Date >= now.Date)
            {
                this.allEvents.RemoveAt(i);
            }
        }
    }

    private EventViewModel LastEventInstanceBefore(int id, DateTime before)
    {
        EventViewModel lastEvent = null;
        for (int i = 0; i < this.allEvents.Count; i++)
        {
            var current = this.allEvents[i];
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
