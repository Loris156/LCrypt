using System.ComponentModel;
using System.Windows;
using LCrypt.ViewModels;

namespace LCrypt.Views
{
    public sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            var mainViewModel = (MainViewModel)DataContext;
            if (await mainViewModel.CheckClosing())
                Application.Current.Shutdown();
        }
    }
}