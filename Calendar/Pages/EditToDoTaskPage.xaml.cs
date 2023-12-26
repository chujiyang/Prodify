using Calendar.ViewModels;
using Calendar.Views;

namespace Calendar.Pages;

public partial class EditToDoTaskPage : BasePage<EditToDoTaskViewModel>
{
    public EditToDoTaskPage(EditToDoTaskViewModel vm) : base(vm)
	{
		InitializeComponent();
	}
}