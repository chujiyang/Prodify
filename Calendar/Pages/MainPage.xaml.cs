using Calendar.ViewModels;

namespace Calendar.Pages;

public partial class MainPage : BasePage<MainViewModel>
{
    public MainPage(MainViewModel vm) : base(vm)
    {
        InitializeComponent();

        this.Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        if (tabView != null)
        {
            tabView.SelectedIndex = AppPreferences.VisibleTab;
            tabView.SelectionChanged += TabView_SelectionChanged;
        }
    }

    private void TabView_SelectionChanged(object sender, Syncfusion.Maui.TabView.TabSelectionChangedEventArgs e)
    {
        AppPreferences.VisibleTab = (int)e.NewIndex;
    }
}

