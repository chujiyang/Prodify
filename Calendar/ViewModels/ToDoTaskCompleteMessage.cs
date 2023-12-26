using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Calendar.ViewModels;

internal class ToDoTaskCompleteMessage : ValueChangedMessage<int>
{
    public ToDoTaskCompleteMessage(int taskId) : base(taskId)
    {
    }
}
