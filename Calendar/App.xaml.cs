using Syncfusion.Maui.Themes;

namespace Calendar;

public partial class App : Application
{
	public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjc1ODA0MkAzMjMzMmUzMDJlMzBHTEw3ZXNidzNxWXZvRmpTYmFkeHFSRDRjczZYTFVjSnA2OXE3TWR3d3RBPQ==");

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
