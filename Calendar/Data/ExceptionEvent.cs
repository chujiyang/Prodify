using SQLite;

namespace Calendar.Data;

public class ExceptionEvent
{
    /// <summary>
    /// Gets or sets the value to display the id.
    /// </summary>
    [PrimaryKey, AutoIncrement, Column("_id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the start time of the event.
    /// </summary>
    [Indexed]
    public DateTime ExceptionTime { get; set; }

    /// <summary>
    /// Gets or set the original event id.
    /// </summary>
    public int EventId { get; set; }
}
