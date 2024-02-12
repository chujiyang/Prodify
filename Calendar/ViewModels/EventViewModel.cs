using Calendar.Data;
using Calendar.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Calendar.ViewModels;

public partial class EventViewModel : ObservableObject, IComparable<EventViewModel>
{
    #region Fields

    /// <summary>
    /// Gets or sets the value to display the id.
    /// </summary>
    [ObservableProperty]
    private int id;

    /// <summary>
    /// Gets or sets the date of the event.
    /// </summary>
    [ObservableProperty]
    private DateTime date;

    /// <summary>
    /// Gets or sets the value to display the start time.
    /// </summary>
    [ObservableProperty]
    private TimeSpan from;

    /// <summary>
    /// Gets or sets the value to display the end time.
    /// </summary>
    [ObservableProperty]
    private TimeSpan to;

    [ObservableProperty]
    private DateTime startTime;

    [ObservableProperty]
    private DateTime endTime;

    /// <summary>
    /// Gets or sets the value to display the subject.
    /// </summary>
    [ObservableProperty]
    private string eventName;

    /// <summary>
    /// Gets or sets the value to display the notes.
    /// </summary>
    [ObservableProperty]
    private string notes;

    /// <summary>
    /// Gets or sets whether it is all day event.
    /// </summary>
    [ObservableProperty]
    private bool isAllDay;

    /// <summary>
    /// Gets or sets the recurrence frequency id.
    /// </summary>
    [ObservableProperty]
    private int recurrenceFrequencyId;

    /// <summary>
    /// Gets or sets the recurrence end time.
    /// </summary>
    [ObservableProperty]
    private DateTime recurrenceEndTime;

    [ObservableProperty]
    private int recurrencePattern;

    [ObservableProperty]
    private int recurrenceInterval;

    /// <summary>
    /// Gets or set the flag whether the task is finished.
    /// </summary>
    [ObservableProperty]
    private bool isFinished;

    [ObservableProperty]
    private bool isNotesVisible;

    [ObservableProperty]
    private AlertType alertType;

    [ObservableProperty]
    bool isRecurring;

    [ObservableProperty]
    int linkedId;

    /// <summary>
    /// Gets or sets the created time.
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets the finished time.
    /// </summary>
    public DateTime? FinishedTime { get; set; }

    #endregion

    #region Constructor

    public EventViewModel()
    {
        var time = DateTime.Now.RoundUpToQuarterHour();
        this.id = 0;
        this.startTime = time;
        this.endTime = time.Add(TimeSpan.FromMinutes(30));
        this.date = time.Date;
        this.from = time - date;
        this.to = this.From.Add(new TimeSpan(0, 30, 0));
        this.eventName = string.Empty;
        this.isAllDay = false;
        this.notes = string.Empty;
        this.recurrenceFrequencyId = 0;
        this.recurrenceEndTime = date;
        this.recurrenceInterval = 1;
        this.recurrencePattern = 0;
        this.isFinished = false;
        this.isNotesVisible = false;
        this.alertType = AlertType.NoAlert;
        this.isRecurring = false;
        this.linkedId = 0;
        this.CreatedTime = time;
        this.FinishedTime = null;

        this.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.RecurrenceFrequencyId))
        {
            this.IsRecurring = this.RecurrenceFrequencyId != 0;
        }
    }

    public EventViewModel(EventViewModel other)
    {
        this.id = other.Id;
        this.startTime = other.StartTime;
        this.endTime = other.EndTime;
        this.date = other.Date;
        this.from = other.From;
        this.to = other.To;
        this.eventName = other.EventName;
        this.isAllDay = false;
        this.notes = other.Notes;
        this.recurrenceFrequencyId = other.RecurrenceFrequencyId;
        this.recurrenceEndTime = other.recurrenceEndTime;
        this.recurrencePattern = other.recurrencePattern;
        this.recurrenceInterval = other.recurrenceInterval;
        this.isFinished = other.IsFinished;
        this.isNotesVisible = other.IsNotesVisible;
        this.alertType = other.AlertType;
        this.isRecurring = this.recurrenceFrequencyId != 0;
        this.linkedId = other.LinkedId;
        this.isRecurring = other.IsRecurring;
        this.CreatedTime = other.CreatedTime;
        this.FinishedTime = other.FinishedTime;
        this.PropertyChanged += OnPropertyChanged;
    }

    public EventViewModel(Event theEvent, DateTime date)
    {
        date = date.FirstSecondOfDate();
        this.id = theEvent.Id;
        this.startTime = theEvent.From;
        this.endTime = theEvent.To;
        this.date = theEvent.From.ChangeDate(date);
        this.from = theEvent.From.GetTimeSpanInTime();
        this.to = theEvent.To.GetTimeSpanInTime();
        this.eventName = theEvent.EventName;
        this.isAllDay = theEvent.IsAllDay;
        this.notes = theEvent.Notes;
        this.recurrenceFrequencyId = theEvent.RecurrenceFrequencyId;
        this.recurrenceEndTime = theEvent.RecurrenceEndTime;
        this.recurrenceInterval = theEvent.RecurrenceInterval;
        this.recurrencePattern = theEvent.RecurrencePattern;
        this.isFinished = theEvent.IsFinished;
        this.isNotesVisible = false;
        this.alertType = (AlertType)(theEvent.AlertType);
        this.isRecurring = this.recurrenceFrequencyId != 0;
        this.CreatedTime = theEvent.CreatedTime;
        this.FinishedTime = theEvent.FinishedTime;

        this.PropertyChanged += OnPropertyChanged;
    }

    public EventViewModel(Event theEvent)
    {
        this.id = theEvent.Id;
        this.startTime = theEvent.From;
        this.endTime = theEvent.To;
        this.date = theEvent.From.Date;
        this.from = theEvent.From.GetTimeSpanInTime();;
        this.to = theEvent.To.GetTimeSpanInTime();
        this.eventName = theEvent.EventName;
        this.isAllDay = theEvent.IsAllDay;
        this.notes = theEvent.Notes;
        this.recurrenceFrequencyId = theEvent.RecurrenceFrequencyId;
        this.recurrenceEndTime = theEvent.RecurrenceEndTime;
        this.recurrenceInterval = theEvent.RecurrenceInterval;
        this.recurrencePattern = theEvent.RecurrencePattern;
        this.isFinished = theEvent.IsFinished;
        this.isNotesVisible = false;
        this.alertType = (AlertType)(theEvent.AlertType);
        this.isRecurring = this.recurrenceFrequencyId != 0;
        this.CreatedTime = theEvent.CreatedTime;
        this.FinishedTime = theEvent?.FinishedTime;

        this.PropertyChanged += OnPropertyChanged;
    }

    public Event ToEvent()
    {
        return new Event
        {
            From = this.StartTime,
            To = this.EndTime,
            EventName = this.EventName,
            Notes = this.Notes,
            IsAllDay = this.IsAllDay,
            Id = this.Id,
            RecurrenceFrequencyId = this.RecurrenceFrequencyId,
            RecurrenceEndTime = this.RecurrenceEndTime,
            RecurrencePattern = this.RecurrencePattern,
            RecurrenceInterval = this.RecurrenceInterval,
            LinkedId = this.LinkedId,
            IsFinished = this.IsFinished,
            AlertType = (int)(this.AlertType),
            CreatedTime = this.CreatedTime,
            FinishedTime = this.FinishedTime
        };
    }

    public bool IsRecurringPattenSame(EventViewModel theOther)
    {
        if (theOther.RecurrenceFrequencyId != RecurrenceFrequencyId)
        {
            return false;
        }
        if (theOther.RecurrencePattern != RecurrencePattern)
        {
            return false;
        }
        if (theOther.RecurrenceInterval != RecurrenceInterval)
        {
            return false;
        }

        return true;
    }

    public bool IsAlertTypeSame(EventViewModel theOther)
    {
        return theOther.AlertType == AlertType;
    }

    public int CompareTo(EventViewModel other)
    {
        int cmp = From.CompareTo(other.From);
        if (cmp == 0)
        {
            cmp = To.CompareTo(other.To);
        }

        return cmp;
    }

    #endregion
}