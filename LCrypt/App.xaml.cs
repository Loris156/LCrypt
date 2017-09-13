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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();
            window.ShowDialog();
        }
    }
}
