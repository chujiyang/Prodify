using SQLite;

namespace Calendar.Data;

public class ToDoList
{
    [PrimaryKey, AutoIncrement, Column("_id")]
    public int Id { get; set; }

    public string Content { get; set; }
}
