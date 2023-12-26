using Calendar.Data;
using Calendar.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Calendar.ViewModels;

public partial class EventViewModel : ObservableObject
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

    /// <summary>
    /// Gets or set the flag whether the task is finished.
    /// </summary>
    [ObservableProperty]
    private bool isFinished;

    [ObservableProperty]
    private bool isNotesVisible;
    #endregion

    #region Constructor

    public EventViewModel()
    {
        var time = DateTime.Now.RoundUpToQuarterHour();
        this.id = 0;
        this.date = time.Date;
        this.from = time - date;
        this.to = this.From.Add(new TimeSpan(0, 30, 0));
        this.eventName = string.Empty;
        this.isAllDay = false;
        this.notes = string.Empty;
        this.recurrenceFrequencyId = 0;
        this.recurrenceEndTime = date;
        this.isFinished = false;
        this.isNotesVisible = false;
    }

    public EventViewModel(EventViewModel other)
    {
        this.id = other.Id;
        this.date = other.Date;
        this.from = other.From;
        this.to = other.To;
        this.eventName = other.EventName;
        this.isAllDay = false;
        this.notes = other.Notes;
        this.recurrenceFrequencyId = other.RecurrenceFrequencyId;
        this.recurrenceEndTime = other.recurrenceEndTime;
        this.isFinished = other.IsFinished;
        this.isNotesVisible = other.IsNotesVisible;
    }

    public EventViewModel(Event theEvent)
    {
        this.id = theEvent.Id;
        this.date = theEvent.From.Date;
        this.from = theEvent.From - this.date;
        this.to = theEvent.To - this.date;
        this.eventName = theEvent.EventName;
        this.isAllDay = theEvent.IsAllDay;
        this.notes = theEvent.Notes;
        this.recurrenceFrequencyId = theEvent.RecurrenceFrequencyId;
        this.recurrenceEndTime = theEvent.RecurrenceEndTime;
        this.isFinished = theEvent.IsFinished;
        this.isNotesVisible = false;
    }

    public Event ToEvent()
    {
        return new Event
        {
            From = this.Date + this.From,
            To = this.Date + this.To,
            EventName = this.EventName,
            Notes = this.Notes,
            IsAllDay = this.IsAllDay,
            Id = this.Id,
            RecurrenceFrequencyId = this.RecurrenceFrequencyId,
            RecurrenceEndTime = this.RecurrenceEndTime,
            IsFinished = this.IsFinished
        };
    }

    #endregion
}