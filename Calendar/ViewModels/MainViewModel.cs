using CommunityToolkit.Mvvm.Input;

namespace Calendar.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    public MainViewModel()
    {
    }

    [RelayCommand]
    public void Agenda()
    {
    }

    [RelayCommand]
    public void ToDo()
    {
    }

    [RelayCommand]
    public async Task Timer()
    {
        await Shell.Current.GoToAsync($"//MainPage/StartTimerPage");
    }
}
