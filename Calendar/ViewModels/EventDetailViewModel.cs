using Calendar.Data;
using Calendar.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Calendar.ViewModels;

[QueryProperty("IsEditingSeries", "IsEditingSeries")]
[QueryProperty("DialogTitle", "DialogTitle")]
[QueryProperty("OperatingEvent", "OperatingEvent")]
public partial class EventDetailViewModel : BaseViewModel
{
    [ObservableProperty]
    EventViewModel operatingEvent;

    /// <summary>
    /// Gets or sets the schedule min date time.    /// </summary>
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

    [ObservableProperty]
    bool isEditingSeries;

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

        WeakReferenceMessenger.Default.Register<RecurrencyUpdateMessage>(this, (r, m) =>
        {
            if (m != null && m.Value != null && OperatingEvent != null)
            {
                int recurrencyPattern = 0;
                foreach (var day in m.Value.SelectedDaysOfWeek)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (day == RecurrencySelectionViewModel.s_DaysOfWeeks[i])
                        {
                            recurrencyPattern |= (1 << i);
                            break;
                        }
                    }
                }

                OperatingEvent.RecurrenceFrequencyId = (int)RecurrenceType.DayOfWeek;
                OperatingEvent.RecurrencePattern = recurrencyPattern;
            }
        });

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
    void UpdateAlert(string message)
    {
        AlertType alertType = AlertType.NoAlert;
        if (Enum.TryParse<AlertType>(message, out alertType))
        {
            OperatingEvent.AlertType = alertType;
        }
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Submit()
    {
        UnsubscribeNotification();
        OperatingEvent.EventName = OperatingEvent.EventName.Trim();
        OperatingEvent.Notes = OperatingEvent.Notes.Trim();

        WeakReferenceMessenger.Default.Send(new EventInsertOrUpdateMessage(OperatingEvent) { IsEditingSeries = this.IsEditingSeries});

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Cancel()
    {
        UnsubscribeNotification();
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async System.Threading.Tasks.Task UpdateRepeat()
    {
        if (OperatingEvent == null)
        {
            return;
        }

        var selectedDays = new ObservableCollection<object>();
        if (OperatingEvent.RecurrenceFrequencyId != 0)
        {
            for (int i = 0; i < 7; i++)
            {
                if ((OperatingEvent.RecurrencePattern & (1 << i)) != 0)
                {
                    selectedDays.Add(RecurrencySelectionViewModel.s_DaysOfWeeks[i]);
                }
            }
        }

        var navigationParameter = new Dictionary<string, object>
        {
            {"SelectedDaysOfWeek", selectedDays}
        };

        await Shell.Current.GoToAsync($"//MainPage/EventDetailPage/RecurrencySelectionPage", navigationParameter);
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
