using Calendar.Data;
using Calendar.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Calendar.ViewModels;

public partial class SeriesViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isEditingSeries;

    [ObservableProperty]
    bool showEditSeries;
}

[QueryProperty("SeriesViewModel", "SeriesViewModel")]
[QueryProperty("OperatingEvent", "OperatingEvent")]
public partial class EventDetailViewModel : BaseViewModel
{
    [ObservableProperty]
    EventViewModel operatingEvent;

    [ObservableProperty]
    SeriesViewModel seriesViewModel;

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
        seriesViewModel = new SeriesViewModel();

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

                OperatingEvent.RecurrenceFrequencyId = (recurrencyPattern != 0) ? (int)RecurrenceType.DayOfWeek : 0;
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
    void EditingSeriesOrOccurance(string message)
    {
        SeriesViewModel.IsEditingSeries = message == "True";
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Submit()
    {
        UnsubscribeNotification();

        OperatingEvent.EventName = OperatingEvent.EventName.Trim();
        OperatingEvent.Notes = OperatingEvent.Notes.Trim();
        OperatingEvent.StartTime = OperatingEvent.Date.ChangeTime(OperatingEvent.From);

        if (OperatingEvent.IsNewEvent)
        {
            OperatingEvent.EndTime = OperatingEvent.Date.ChangeTime(OperatingEvent.To);
            
            OperatingEvent.CreatedTime = DateTime.Now;
            if (OperatingEvent.IsRecurring)
            {
                OperatingEvent.EndTime = OperatingEvent.EndTime.AddYears(1);
            }
        }
        else if (!SeriesViewModel.IsEditingSeries || !OperatingEvent.IsRecurring)
        {
            OperatingEvent.EndTime = OperatingEvent.Date.ChangeTime(OperatingEvent.To);
        }
        else
        {
            OperatingEvent.EndTime = OperatingEvent.Date.ChangeTime(OperatingEvent.To).AddYears(1);
        }

        if (!OperatingEvent.IsNewEvent && !SeriesViewModel.IsEditingSeries)
        {
            OperatingEvent.RecurrenceFrequencyId = 0;
            OperatingEvent.RecurrencePattern = 0;
            OperatingEvent.RecurrenceInterval = 0;
        }

        OperatingEvent.Duration = OperatingEvent.CalculateDuration();

        WeakReferenceMessenger.Default.Send(new EventInsertOrUpdateMessage(OperatingEvent) { IsEditingSeries = SeriesViewModel.IsEditingSeries});

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
        if (oldFrom == null)
        {
            return;
        }
        var oldTo = OperatingEvent.To;
        OperatingEvent.To = OperatingEvent.From + (oldTo - oldFrom.Value);
        oldFrom = OperatingEvent.From;
    }

    private void EnsureTo()
    {
        if (OperatingEvent.Date + OperatingEvent.To < OperatingEvent.Date + OperatingEvent.From + TimeSpan.FromMinutes(5))
        {
            OperatingEvent.To = OperatingEvent.From + TimeSpan.FromMinutes(5);
        }
    }
}
