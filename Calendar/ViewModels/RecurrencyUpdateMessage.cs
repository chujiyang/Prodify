using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Calendar.ViewModels;

public class RecurrencyUpdateMessage : ValueChangedMessage<RecurrencySelectionViewModel>
{
    public RecurrencyUpdateMessage(RecurrencySelectionViewModel value) : base(value)
    {
    }
}
