using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using LCrypt.Properties;
using MahApps.Metro;

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
            if (Settings.UpgradeRequired)
            {
                Settings.Upgrade();
                Settings.UpgradeRequired = false;
            }

            if (e.Args.Length == 1)
                if (File.Exists(e.Args[0]))
                {
                    //TODO: Implement open file from argument.
                }
            if (e.Args.Length > 1)
            {
                MessageBox.Show("LCrypt wurde mit zu vielen Argumenten aufgerufen", "LCrypt", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation, MessageBoxResult.OK);
                Shutdown(0);
            }

            if (!string.IsNullOrWhiteSpace(Settings.Language))
            {
                CultureInfo.CurrentCulture = new CultureInfo(Settings.Language);
                CultureInfo.CurrentUICulture = new CultureInfo(Settings.Language);
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Settings.Language);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Settings.Language);
            }

            if (string.IsNullOrWhiteSpace(Settings.Accent))
                Settings.Accent = "Blue";
            if (string.IsNullOrWhiteSpace(Settings.Theme))
                Settings.Theme = "BaseDark";

            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(Settings.Accent),
                ThemeManager.GetAppTheme(Settings.Theme));
            var window = new MainWindow();
            window.Show();
        }
    }
}
