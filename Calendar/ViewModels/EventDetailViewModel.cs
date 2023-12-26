using Calendar.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Calendar.ViewModels;

[QueryProperty("DialogTitle", "DialogTitle")]
[QueryProperty("OperatingEvent", "OperatingEvent")]
public partial class EventDetailViewModel : BaseViewModel
{
    [ObservableProperty]
    EventViewModel operatingEvent;

    /// <summary>
    /// Gets or sets the schedule min date time.
    /// </summary>
    [ObservableProperty]
    DateTime minDisplayDateTime;

    /// <summary>
    /// Gets or sets the schedule max date time.
    /// </summary>
    [ObservableProperty]
    DateTime maxDisplayDateTime;

    [ObservableProperty]
    bool isDateOpen;

    [ObservableProperty]
    bool isFromOpen;

    [ObservableProperty]
    bool isToOpen;

    [ObservableProperty]
    string dialogTitle;

    private bool subscribedNotification = false;
    private DateTime minDateTime;
    private DateTime maxDateTime;
    private TimeSpan? oldFrom;

    public EventDetailViewModel()
    {       
        minDateTime = DateTime.Now.RoundUpToQuarterHour();
        maxDateTime = minDateTime.AddMonths(3);
        minDisplayDateTime = new DateTime(minDateTime.Year, 1, 1);
        maxDisplayDateTime = new DateTime(maxDateTime.Year, 12, 31);
        isDateOpen = false;
        isFromOpen = false;
        isToOpen = false;
        dialogTitle = "Edit Task";
    }

    [RelayCommand]
    void EditDate()
    {
        SubscribeNotification();
        IsDateOpen = true;
    }

    [RelayCommand]
    void EditFrom()
    {
        SubscribeNotification();

        oldFrom = OperatingEvent.From;        
        IsFromOpen = true;
    }

    [RelayCommand]
    void EditTo()
    {
        SubscribeNotification();

        IsToOpen = true;
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Submit()
    {
        UnsubscribeNotification();
        WeakReferenceMessenger.Default.Send(new EventInsertOrUpdateMessage(OperatingEvent));

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Cancel()
    {
        UnsubscribeNotification();
        await Shell.Current.GoToAsync("..");
    }

    private void SubscribeNotification()
    {
        if (!subscribedNotification)
        {
            subscribedNotification = true;
            OperatingEvent.PropertyChanged += OperatingEvent_PropertyChanged;
        }
    }

    private void UnsubscribeNotification()
    {
        if (subscribedNotification)
        {
            subscribedNotification = false;
            OperatingEvent.PropertyChanged -= OperatingEvent_PropertyChanged;
        }
    }


    private void OperatingEvent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Date")
        {
            EnsureDate();
            EnsureFrom();
            EnsureTo();
        }
        else if (e.PropertyName == "From")
        {
            EnsureFrom();
            EnsureTo();
        }
        else if (e.PropertyName == "To")
        {
            EnsureTo();
        }
    }

    private void EnsureDate()
    {
    }

    private void EnsureFrom()
    {       
        if (oldFrom != null)
        {
            var oldTo = OperatingEvent.To;
            OperatingEvent.To = OperatingEvent.From + (oldTo - oldFrom.Value);
            oldFrom = OperatingEvent.From;
        }
    }

    private void EnsureTo()
    {
        if (OperatingEvent.Date + OperatingEvent.To < OperatingEvent.Date + OperatingEvent.From + TimeSpan.FromMinutes(15))
        {
            OperatingEvent.To = OperatingEvent.From + TimeSpan.FromMinutes(15);
        }
    }
}
