using Syncfusion.Maui.Themes;

namespace Calendar;

public partial class App : Application
{
	public App()
	{
        // Syncfusion 23.1.36
        // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjc1ODA0MkAzMjMzMmUzMDJlMzBHTEw3ZXNidzNxWXZvRmpTYmFkeHFSRDRjczZYTFVjSnA2OXE3TWR3d3RBPQ==");

        // Syncfusion 24.1.41
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzAwMDYyMEAzMjM0MmUzMDJlMzBGWEt2MkxVUnJJekJoNU50MXZSc1lwbktGYml6SjRIZkc0aUkrL0VBTE9vPQ==");

        //App.Current.UserAppTheme = AppTheme.Dark;

        Application.Current.RequestedThemeChanged += (s, a) =>
        {
            UpdateTheme(a.RequestedTheme);
        };

        InitializeComponent();
        UpdateTheme(Application.Current.UserAppTheme);

        MainPage = new AppShell();
	}

    private void UpdateTheme(AppTheme requestedTheme)
    {
        ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        var dictionary = mergedDictionaries.FirstOrDefault();
        if (dictionary != null)
        {
            if (requestedTheme == AppTheme.Dark)
            {
                Object pickerBackground;
                if (dictionary.TryGetValue("DarkPickerBackground", out pickerBackground))
                {
                    dictionary["SfPopupNormalMessageBackground"] = pickerBackground;
                }
            }
            else if (requestedTheme == AppTheme.Light)
            {
                Object pickerBackground;
                if (dictionary.TryGetValue("LightPickerBackground", out pickerBackground))
                {
                    dictionary["SfPopupNormalMessageBackground" ] = pickerBackground;
                }
            }
        }
    }
}
