using Calendar.Data;
using Calendar.Pages;
using Calendar.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace Calendar;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
        builder.ConfigureSyncfusionCore();
        builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddSingleton<DatabaseContext>();
        builder.Services.AddSingleton<EventListViewModel>();
        builder.Services.AddSingleton<ToDoTaskListViewModel>();

        RegisterViewsAndViewModels(builder.Services);
        builder.Services.AddSingleton<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		var app = builder.Build();

        ServiceHelper.Initialize(app.Services);

        return app;
    }

    static void RegisterViewsAndViewModels(in IServiceCollection services)
    {
        services.AddTransientWithShellRoute<MainPage, MainViewModel>(AppShell.GetRoute<MainPage, MainViewModel>());
        services.AddTransientWithShellRoute<EventDetailPage, EventDetailViewModel>(AppShell.GetRoute<EventDetailPage, EventDetailViewModel>());
        services.AddTransientWithShellRoute<TaskTimerPage, TaskTimerViewModel>(AppShell.GetRoute<TaskTimerPage, TaskTimerViewModel>());
        services.AddTransientWithShellRoute<EditToDoTaskPage, EditToDoTaskViewModel>(AppShell.GetRoute<EditToDoTaskPage, EditToDoTaskViewModel>());
    }
}
