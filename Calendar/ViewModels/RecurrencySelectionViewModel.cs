using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Calendar.ViewModels;

//public partial class RecurrencyItem : ObservableObject
//{
//    [ObservableProperty]
//    string week;
//}

[QueryProperty("SelectedDaysOfWeek", "SelectedDaysOfWeek")]
public partial class RecurrencySelectionViewModel : BaseViewModel
{
    public static ReadOnlyCollection<string> s_DaysOfWeeks = new ReadOnlyCollection<string>(    
        new [] {
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
        });

    //[ObservableProperty]
    //ObservableCollection<RecurrencyItem> daysOfWeek;

    [ObservableProperty]
    ObservableCollection<string> daysOfWeek;


    [ObservableProperty]
    ObservableCollection<object> selectedDaysOfWeek;

    [ObservableProperty]
    DateTime minDisplayDate;

    [ObservableProperty]
    DateTime maxDisplayDate;

    [ObservableProperty]
    DateTime endDate;

    [ObservableProperty]
    bool hasEndDate;

    [ObservableProperty]
    bool isDateOpen;

    public RecurrencySelectionViewModel()
    {
        //daysOfWeek = new ObservableCollection<RecurrencyItem>(new[]
        //{
        //    new RecurrencyItem { Week = "Sunday" },
        //    new RecurrencyItem { Week = "Monday" },
        //    new RecurrencyItem { Week = "Tuesday" },
        //    new RecurrencyItem { Week = "Wednesday" },
        //    new RecurrencyItem { Week = "Thursday" },
        //    new RecurrencyItem { Week = "Friday" },
        //    new RecurrencyItem { Week = "Saturday" }
        //});

        daysOfWeek = new ObservableCollection<string>(new[]
        {
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
        });

        selectedDaysOfWeek = new ObservableCollection<object>();

        minDisplayDate = DateTime.Today;
        maxDisplayDate = minDisplayDate.AddDays(90);
        hasEndDate = false;
        isDateOpen = false;
    }

    [RelayCommand]
    async System.Threading.Tasks.Task Back()
    {
        WeakReferenceMessenger.Default.Send(new RecurrencyUpdateMessage(this));

        await Shell.Current.GoToAsync("..");
    }
}
