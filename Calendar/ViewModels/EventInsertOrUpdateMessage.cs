using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Calendar.ViewModels
{
    internal class EventInsertOrUpdateMessage : ValueChangedMessage<EventViewModel>
    {
        public EventInsertOrUpdateMessage(EventViewModel value) : base(value)
        {
        }

        public bool IsEditingSeries { get; set; }
    }
}
