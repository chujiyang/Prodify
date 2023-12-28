using Syncfusion.Maui.Themes;

namespace Calendar;

public partial class App : Application
{
	public App()
	{
// Syncfusion 23.1.36
//        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjc1ODA0MkAzMjMzMmUzMDJlMzBHTEw3ZXNidzNxWXZvRmpTYmFkeHFSRDRjczZYTFVjSnA2OXE3TWR3d3RBPQ==");

        // Syncfusion 24.1.41
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzAwMDYyMEAzMjM0MmUzMDJlMzBGWEt2MkxVUnJJekJoNU50MXZSc1lwbktGYml6SjRIZkc0aUkrL0VBTE9vPQ==");

       // App.Current.UserAppTheme = AppTheme.Dark;

        InitializeComponent();
        UpdateTheme();

        MainPage = new AppShell();
	}

    private void UpdateTheme()
    {
        ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        var dictionary = mergedDictionaries.FirstOrDefault();
        if (dictionary != null)
        {
            if (Application.Current.UserAppTheme == AppTheme.Dark)
            {
                Object pickerBackground;
                if (dictionary.TryGetValue("DarkPickerBackground", out pickerBackground))
                {
                    dictionary.Add("SfPopupNormalMessageBackground", pickerBackground);
                }
            }
            else
            {
                Object pickerBackground;
                if (dictionary.TryGetValue("LightPickerBackground", out pickerBackground))
                {
                    dictionary.Add("SfPopupNormalMessageBackground", pickerBackground);
                }
            }
        }
    }
}
