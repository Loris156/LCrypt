using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

    public partial class PasswordManagerWindow : INotifyPropertyChanged
    {
        private PasswordStorage _storage;
        private StorageEntry _selectedEntry;
        private string _displayedPassword;

        public PasswordManagerWindow(PasswordStorage storage)
        {
            InitializeComponent();

            DataContext = this;

            _storage = storage;
            DisplayedEntries = new ObservableCollection<StorageEntry>(_storage.Entries);

        }

        public ObservableCollection<StorageEntry> DisplayedEntries { get; set; }

        public StorageEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged(nameof(SelectedEntry));
            }
        }

        public string DisplayedPassword
        {
            get => _displayedPassword;
            set
            {
                _displayedPassword = value;
                OnPropertyChanged(nameof(DisplayedPassword));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedEntry = null;
        }
    }
}
