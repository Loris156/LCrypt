using LCrypt.ViewModels;
using LCrypt.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using LCrypt.Utility;
using Standard;

namespace LCrypt
{
    public partial class App
    {
        public static ResourceDictionary LocalizationDictionary { get; private set; }
        public static ResourceDictionary DialogDictionary { get; private set; }

        public static string MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static TaskbarProgressManager TaskbarProgressManager { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(!Directory.Exists(Path.Combine(MyDocuments, "LCrypt")))
            {
                Directory.CreateDirectory(Path.Combine(MyDocuments, "LCrypt"));
                Directory.CreateDirectory(Path.Combine(MyDocuments, "LCrypt", "Backups"));
            }
            else if(!Directory.Exists(Path.Combine(MyDocuments, "LCrypt", "Backups")))
                Directory.CreateDirectory(Path.Combine(MyDocuments, "LCrypt", "Backups"));

            LocalizationDictionary = Resources.MergedDictionaries[0];

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            DialogDictionary = new ResourceDictionary
            {
                Source = new Uri(
                    "pack://application:,,,/MaterialDesignThemes.MahApps;component/Themes/MaterialDesignTheme.MahApps.Dialogs.xaml")
            };
            var window = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            TaskbarProgressManager = new TaskbarProgressManager(window.TaskbarItemInfo);
            window.ShowDialog();
        }
    }
}
