using Calendar.ViewModels;
using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;

namespace Calendar.Behaviors;


public class EventListGroupComparer : IComparer<GroupResult>
{
    public int Compare(GroupResult x, GroupResult y)
    {
        var lastX = x.GetGroupLastItem() as EventViewModel;
        var lastY = y.GetGroupLastItem() as EventViewModel;

        if (lastX.Date < lastY.Date)
        {
            return -1;
        }
        else if (lastX.Date > lastY.Date)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}

public class EventListViewGroupingBehavior : Behavior<Syncfusion.Maui.ListView.SfListView>
{
    #region Fields

    private Syncfusion.Maui.ListView.SfListView ListView;

    #endregion

    #region Overrides
    protected override void OnAttachedTo(Syncfusion.Maui.ListView.SfListView bindable)
    {
        ListView = bindable;
        ListView.DataSource.GroupDescriptors.Add(
            new GroupDescriptor()
            {
                PropertyName = "Date",
                KeySelector = (object obj1) =>
                {
                    var item = (obj1 as ViewModels.EventViewModel);
                    return item.Date.ToString("MMM dd, yyyy, ddd");
                },
                Comparer = new EventListGroupComparer()
            });
        ListView.DataSource.SortDescriptors.Add(new SortDescriptor()
        {
            PropertyName = "From",
            Direction = ListSortDirection.Ascending

        });
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Syncfusion.Maui.ListView.SfListView bindable)
    {
        ListView = null;
        base.OnDetachingFrom(bindable);
    }
    #endregion
}
