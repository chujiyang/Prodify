using SQLite;

namespace Calendar.Data;

public class ToDoTask
{
    [PrimaryKey, AutoIncrement, Column("_id")]
    public int Id { get; set; }

    public int Order { get; set; }

    public int Priority { get; set; }

    public string Description { get; set; }

    public bool IsFinished { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime FinishedTime { get; set; }
}
