using Calendar.ViewModels;

namespace Calendar.Pages;

public partial class TaskTimerPage : BasePage<TaskTimerViewModel>
{
	public TaskTimerPage(TaskTimerViewModel vm) : base(vm)
	{
		InitializeComponent();
	}
}