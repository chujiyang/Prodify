using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Calendar.ViewModels;

[QueryProperty("OperatingTask", "OperatingTask")]
public partial class EditToDoTaskViewModel : BaseViewModel
{
    [ObservableProperty]
    ToDoTaskViewModel operatingTask;

    public EditToDoTaskViewModel()
    {
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Submit()
    {
        WeakReferenceMessenger.Default.Send(new ToDoTaskInsertOrUpdateMessage(OperatingTask));

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}

