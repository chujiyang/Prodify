using Calendar.ViewModels;

namespace Calendar.Pages;

public partial class RecurrencySelectionPage : BasePage<RecurrencySelectionViewModel>
{
	public RecurrencySelectionPage(RecurrencySelectionViewModel vm) : base(vm)
	{
		InitializeComponent();
	}
}