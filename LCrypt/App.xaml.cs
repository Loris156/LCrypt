using LCrypt.Properties;
using MahApps.Metro;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Windows;
using System.Xml;
using LCrypt.Password_Manager;
using LCrypt.Utility;

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

            var storage = new PasswordStorage();
            storage.Entries.Add(new StorageEntry
            {
                Name = "Microsoft",
                Username = "Loris Leitner",
                Email = "lorisleitner@live.com",
                IconId = 277,
                Password = new byte[23],
                Comment = "Nix"                
            });

            storage.Entries.Add(new StorageEntry
            {
                Name = "League of Legends",
                Username = "Loris156",
                Email = "lorisleitner@gmail.com",
                IconId = 43,
                Password = new byte[43],
                Comment = "Mein Main Account"
            });

            //var ms = new MemoryStream();
            //{
            //    var xml = XmlDictionaryWriter.CreateBinaryWriter(ms);
            //    var dcs = new DataContractSerializer(typeof(PasswordStorage));
            //    dcs.WriteObject(xml, storage);
            //    xml.Flush();
            //}
            //ms.Position = 0;
            //{
            //    var xml = XmlDictionaryReader.CreateBinaryReader(ms, XmlDictionaryReaderQuotas.Max);
            //    var dcs = new DataContractSerializer(typeof(PasswordStorage));
            //    var obj = dcs.ReadObject(xml) as PasswordStorage;
            //}



            if (e.Args.Length == 1)
            {
                if (File.Exists(e.Args[0]))
                {
                    //TODO: Implement open file from argument.
                }
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

            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(Settings.Accent), ThemeManager.GetAppTheme(Settings.Theme));
           // var window = new MainWindow();
            var window = new PasswordManagerWindow(storage);
            window.Show();
        }
    }
}
