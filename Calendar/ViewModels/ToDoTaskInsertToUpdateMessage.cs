using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Calendar.ViewModels;

internal class ToDoTaskInsertOrUpdateMessage : ValueChangedMessage<ToDoTaskViewModel>
{
    public ToDoTaskInsertOrUpdateMessage(ToDoTaskViewModel value) : base(value)
    {
    }
}
