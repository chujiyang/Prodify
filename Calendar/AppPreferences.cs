namespace Calendar;

public static class AppPreferences
{
    public enum VisibleTabs
    {
        Agenda = 0,
        ToDo = 1
    }

    public static int VisibleTab
    {
        get
        {
            return Preferences.Get("VisibleTab", (int)VisibleTabs.Agenda);
        }
        set
        {
            Preferences.Set("VisibleTab", value);
        }
    }
}
