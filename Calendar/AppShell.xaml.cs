using Calendar.ViewModels;
using Calendar.Pages;

namespace Calendar;

public partial class AppShell : Shell
{
    static readonly IReadOnlyDictionary<Type, string> pageRouteMappingDictionary = new Dictionary<Type, string>(new[]
    {
        CreateRoutePageMapping<MainPage, MainViewModel>(),
        CreateRoutePageMapping<EventDetailPage, EventDetailViewModel>(),
        CreateRoutePageMapping<TaskTimerPage, TaskTimerViewModel>(),
        CreateRoutePageMapping<EditToDoTaskPage, EditToDoTaskViewModel>()
    });

    public AppShell()
	{
		InitializeComponent();
    }


    public static string GetRoute<TPage, TViewModel>() where TPage : BasePage<TViewModel>
                                                        where TViewModel : BaseViewModel
    {
        if (!pageRouteMappingDictionary.TryGetValue(typeof(TPage), out var route))
        {
            throw new KeyNotFoundException($"No map for ${typeof(TPage)} was found on navigation mappings. Please register your ViewModel in {nameof(AppShell)}.{nameof(pageRouteMappingDictionary)}");
        }

        return route;
    }

    static KeyValuePair<Type, string> CreateRoutePageMapping<TPage, TViewModel>() where TPage : BasePage<TViewModel>
                                                                                    where TViewModel : BaseViewModel
    {
        var route = CreateRoute();
        Routing.RegisterRoute(route, typeof(TPage));

        return new KeyValuePair<Type, string>(typeof(TPage), route);

        static string CreateRoute()
        {
            if (typeof(TPage) == typeof(MainPage))
            {
                return $"//{nameof(MainPage)}";
            }

            if (typeof(TPage) == typeof(EventDetailPage))
            {
                return $"//{nameof(MainPage)}/{nameof(EventDetailPage)}";
            }

            if (typeof(TPage) == typeof(TaskTimerPage))
            {
                return $"//{nameof(MainPage)}/{nameof(TaskTimerPage)}";
            }

            if (typeof(TPage) == typeof(EditToDoTaskPage))
            {
                return $"//{nameof(MainPage)}/{nameof(EditToDoTaskPage)}";
            }

            throw new NotSupportedException($"{typeof(TPage)} Not Implemented in {nameof(pageRouteMappingDictionary)}");
        }
    }
}
