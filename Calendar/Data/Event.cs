﻿using Calendar.Extensions;
using SQLite;

namespace Calendar.Data;

public class Event
{
    #region Properties

    /// <summary>
    /// Gets or sets the value to display the id.
    /// </summary>
    [PrimaryKey, AutoIncrement, Column("_id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the start time of the event.
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// Gets or sets the end time of the event.
    /// </summary>
    [Indexed]
    public DateTime To { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether the appointment is all-day or not.
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Gets or sets the value to display the subject.
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// Gets or sets the value to display the notes.
    /// </summary>
    public string Notes { get; set; }

    /// <summary>
    /// Gets or sets the value to display the recurrence frequency id.
    /// </summary>
    public int RecurrenceFrequencyId { get; set; }

    /// <summary>
    /// Gets or sets the recurence end time.
    /// </summary>
    public DateTime RecurrenceEndTime { get; set; }

    /// <summary>
    /// Gets or set the flag whether the task is finished.
    /// </summary>
    public bool IsFinished { get; set; }

    /// <summary>
    /// Gets or sets the duration of the task in minutes
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Gets or set the recurrence pattern.
    /// </summary>
    public int RecurrencePattern { get; set; }

    /// <summary>
    /// Gets or sets the recurrence interval.
    /// </summary>
    public int RecurrenceInterval { get; set; }

    /// <summary>
    /// Gets or set whether send alarming.
    /// </summary>
    public int AlertType { get; set; }

    /// <summary>
    /// Gets or sets the minutes before the reminding.
    /// </summary>
    public int AlertBefore { get; set; }

    /// <summary>
    /// Gets or sets the created time
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets the finished time
    /// </summary>
    public DateTime? FinishedTime { get; set; }

    /// <summary>
    /// Gets and gets the linked event id.
    /// </summary>
    public int LinkedId { get; set; }

    #endregion

    #region Constructor

    public Event()
    {
        Id = 0;
        var time = DateTime.Now;
        From = time.RoundUpToQuarterHour();
        To = From.Add(TimeSpan.FromMinutes(30));
        EventName = string.Empty;
        IsAllDay = false;
        Notes = string.Empty;
        RecurrenceFrequencyId = 0;
        RecurrenceEndTime = To;
        RecurrencePattern = 0;
        RecurrenceInterval = 1;
        IsFinished = false;
        LinkedId = 0;
    }

    #endregion
}
