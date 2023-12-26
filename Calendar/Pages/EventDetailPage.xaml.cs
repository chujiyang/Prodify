using Calendar.ViewModels;
using Syncfusion.Maui.Themes;

namespace Calendar.Pages;

public partial class EventDetailPage : BasePage<EventDetailViewModel>
{
    public EventDetailPage(EventDetailViewModel vm) : base(vm)
	{
        InitializeComponent();

        ICollection<ResourceDictionary> mergedDictionaries = this.Resources.MergedDictionaries;
        if (mergedDictionaries != null)
        {
            mergedDictionaries.Add(new SyncfusionThemeDictionary(true));
        }
    }
}