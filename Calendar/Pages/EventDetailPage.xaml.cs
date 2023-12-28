using Calendar.ViewModels;

namespace Calendar.Pages;

public partial class EventDetailPage : BasePage<EventDetailViewModel>
{
    public EventDetailPage(EventDetailViewModel vm) : base(vm)
	{
        InitializeComponent();
    }
}