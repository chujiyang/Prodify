using CommunityToolkit.Mvvm.Messaging.Messages;

internal class EventCompleteMessage : ValueChangedMessage<int>
{
    public EventCompleteMessage(int taskId) : base(taskId)
    {
    }
}

