using LCrypt.ViewModels;
using LCrypt.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LCrypt
{
    public partial class App
    {
        public static ResourceDictionary LocalizationDictionary { get; private set; }
        public static ResourceDictionary DialogDictionary { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            LocalizationDictionary = Resources.MergedDictionaries[0];
            DialogDictionary = new ResourceDictionary
            {
                Source = new Uri(
                    "pack://application:,,,/MaterialDesignThemes.MahApps;component/Themes/MaterialDesignTheme.MahApps.Dialogs.xaml")
            };
            var window = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            window.ShowDialog();
        }
    }
}
