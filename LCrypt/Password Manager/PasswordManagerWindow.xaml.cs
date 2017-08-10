using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LCrypt.Password_Manager;
using LCrypt.Properties;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace LCrypt.Password_Manager
{

    public partial class PasswordManagerWindow
    {
        private readonly Timer _inactivityTimer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
        private readonly Stopwatch _timeSinceLastUserAction = new Stopwatch();

        private PasswordStorage _passwordStorage;
        public ObservableCollection<StorageEntry> DisplayedEntries { get; } = new ObservableCollection<StorageEntry>();

        public PasswordManagerWindow(PasswordStorage storage)
        {
            InitializeComponent();

            _passwordStorage = storage;
            _passwordStorage.Entries.ForEach(e => DisplayedEntries.Add(e));

            _inactivityTimer.Elapsed += _inactivityTimer_Elapsed;
            _inactivityTimer.Start();

            DisplayedEntries.Add(new StorageEntry
            {
                IconId = 239,
                Username = "Loris156",
                Name = "Mein Konto bei jemandem.",
                Email = "lorisleitner@live.com",
                Comment = "Mein Konto, aber ich weiß nicht wo!asdasdasdsadasdasdsadasdsadasdsadadsad",
                
            });

        }

        private void _inactivityTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_timeSinceLastUserAction.Elapsed > TimeSpan.FromMinutes(1))
                Dispatcher.Invoke(Close);
        }

        private void OnUserAction(object sender, EventArgs e)
        {
            Debug.WriteLine("Close elapsed by " + e);
            _timeSinceLastUserAction.Restart();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DisplayedEntries.Add(new StorageEntry
            {
                Name = "Neu!"
            });

        }

        private void RmResult(object sender, RoutedEventArgs e)
        {
            LbEntries.Items.RemoveAt(LbEntries.Items.Count - 1);
        }

        private void LbEntries_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LbEntries.SelectedItem = null;
        }

        private void LbEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TblPassword.Text = (sender as ListBox).SelectedItem != null ? "••••" : string.Empty;
        }
    }
}
