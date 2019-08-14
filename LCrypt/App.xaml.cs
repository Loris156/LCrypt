using LCrypt.Properties;
using MahApps.Metro;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace LCrypt
{
    public partial class App
    {
        public static Settings Settings { get; } = Settings.Default;

        public static void Restart()
        {
            try
            {
                if (ResourceAssembly.Location != null)
                    Process.Start(ResourceAssembly.Location);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                Current.Shutdown();
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Settings.Language))
            {
                CultureInfo.CurrentCulture = new CultureInfo(Settings.Language);
                CultureInfo.CurrentUICulture = new CultureInfo(Settings.Language);
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Settings.Language);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Settings.Language);
            }

            // Use current culture in XAML
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            if (string.IsNullOrWhiteSpace(Settings.Accent))
                Settings.Accent = "Blue";
            if (string.IsNullOrWhiteSpace(Settings.Theme))
                Settings.Theme = "BaseDark";

            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(Settings.Accent), ThemeManager.GetAppTheme(Settings.Theme));
            var window = new MainWindow();
            window.Show();
        }
    }
}
