using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace LCrypt.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings
    {
        private Settings()
        {
            PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Save();
            if (Debugger.IsAttached)
                Console.WriteLine("Settings saved!");
        }
    }
}
