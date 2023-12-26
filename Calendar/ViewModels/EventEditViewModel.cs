using System.ComponentModel.DataAnnotations;

namespace Calendar.ViewModels;

internal class EventEditViewModel
{
    /// <summary>
    /// Gets or sets the value to display the start date.
    /// </summary>    
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Gets or sets the value to display the end date.
    /// </summary>
    public TimeSpan FromTime { get; set; }

    public DateTime ToDate { get; set; }

    public TimeSpan ToTime { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether the appointment is all-day or not.
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Gets or sets the value to display the subject.
    /// </summary>
    [Required(ErrorMessage = "Please enter task name")]
    public string EventName { get; set; }

    /// <summary>
    /// Gets or sets the value to display the notes.
    /// </summary>
    public string Notes { get; set; }

}
