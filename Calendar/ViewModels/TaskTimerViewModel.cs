
using Calendar.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Calendar.ViewModels;

[QueryProperty("TaskType", "TaskType")]
[QueryProperty("TaskDescription", "TaskDescription")]
[QueryProperty("SelectedHour", "SelectdHour")]
[QueryProperty("SelectedMinute", "SelectedMinute")]
[QueryProperty("SelectedSecond", "SelectedSecond")]
[QueryProperty("Id", "Id")]
public partial class TaskTimerViewModel : BaseViewModel
{
    [ObservableProperty]
    private TaskType taskType;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    string taskDescription;

    [ObservableProperty]
    private ObservableCollection<int> hoursList;

    [ObservableProperty]
    private ObservableCollection<int> minutesList;

    [ObservableProperty]
    private ObservableCollection<int> secondsList;

    [ObservableProperty]
    private int selectedHour;

    [ObservableProperty]
    private int selectedMinute;

    [ObservableProperty]
    private int selectedSecond;

    [ObservableProperty]
    private TimeSpan remainingTime;

    [ObservableProperty]
    private double seconds;

    [ObservableProperty]
    private bool startVisible;

    [ObservableProperty]
    private bool stopVisible;

    [ObservableProperty]
    private bool playVisible;

    [ObservableProperty]
    private bool pauseVisible;

    [ObservableProperty]
    private double totalSeconds;

    [ObservableProperty]
    private bool isTimerPickerOpen = false;

    bool isCircularTimerOn = false;

    IDispatcherTimer dispatcherTimer;

    public TaskTimerViewModel()
    {
        hoursList = new ObservableCollection<int>();
        for (int i = 0; i < 24; i++)
        {
            hoursList.Add(i);
        }
        minutesList = new ObservableCollection<int>();
        for (int i = 0; i < 60; i++)
        {
            minutesList.Add(i);
        }
        secondsList = new ObservableCollection<int>();
        for (int i = 0; i < 60; i++)
        {
            secondsList.Add(i);
        }

        id = 0;
        taskDescription = string.Empty;

        selectedHour = 0;
        selectedMinute = 15;
        selectedSecond = 0;
        remainingTime = TimeSpan.Zero;
        seconds = 0;
        startVisible = true;
        stopVisible = false;
        playVisible = false;
        pauseVisible = false;
        dispatcherTimer = Application.Current.Dispatcher.CreateTimer();
        dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
        dispatcherTimer.Tick += DispatcherTimer_Tick;
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Complete()
    {
        if (TaskType == TaskType.Event)
        {
            WeakReferenceMessenger.Default.Send(new EventCompleteMessage(Id));
        }
        else if (TaskType == TaskType.ToDoTask)
        {
            WeakReferenceMessenger.Default.Send(new ToDoTaskCompleteMessage(Id));
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void Start()
    {
        TotalSeconds = SelectedHour * 3600 + SelectedMinute * 60 + SelectedSecond;
        Seconds = TotalSeconds;
        RemainingTime = TimeSpan.FromSeconds(TotalSeconds);
        isCircularTimerOn = true;
        PlayVisible = false;
        PauseVisible = true;
        StopVisible = true;
        StartVisible = false;
        dispatcherTimer.Start();
    }

    [RelayCommand]
    private void Stop()
    {
        TotalSeconds = SelectedHour * 3600 + SelectedMinute * 60 + SelectedSecond;
        Seconds = TotalSeconds;
        isCircularTimerOn = false;
        PlayVisible = false;
        PauseVisible = false;
        StopVisible = false;
        StartVisible = true;
    }


    private void DispatcherTimer_Tick(object sender, EventArgs e)
    {
        if (!isCircularTimerOn && Seconds > 0)
        {
            return;
        }

        Seconds = Seconds - 1;
        RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
        if (Seconds == 0)
        {
            isCircularTimerOn = false;

            StartVisible = true;
            StopVisible = false;
            PlayVisible = false;
            PauseVisible = false;
            dispatcherTimer.Stop();
        }
    }

    [RelayCommand]
    private void Reset()
    {
    }

    [RelayCommand]
    private void Play()
    {
        PlayVisible = false;
        PauseVisible = true;
        isCircularTimerOn = true;

    }

    [RelayCommand]
    private void Pause()
    {
        PlayVisible = true;
        PauseVisible = false;

        isCircularTimerOn = false;
    }
}
