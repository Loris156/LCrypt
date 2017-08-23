using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LCrypt.Password_Manager;
using LCrypt.Properties;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Localization = LCrypt.Properties.Localization;

namespace LCrypt.Password_Manager
{

    public partial class PasswordManagerWindow : INotifyPropertyChanged
    {
        private PasswordStorage _storage;
        private StorageEntry _selectedEntry;
        private string _displayedPassword;

        private StorageCategory _selectedCategory;

        private ObservableCollection<StorageEntry> _displayedEntries;
        private ObservableCollection<StorageCategory> _categories;

        public PasswordManagerWindow(PasswordStorage storage)
        {
            InitializeComponent();

            DataContext = this;

            _storage = storage;
            DisplayedEntries = new ObservableCollection<StorageEntry>(_storage.Entries);
            Categories = new ObservableCollection<StorageCategory>
            {
                new StorageCategory
                {
                    Name = _storage.Name,
                    IconId = 69,
                    Tag = "all"
                },
                new StorageCategory
                {
                    Name = Localization.Favorites,
                    IconId = 81,
                    Tag = "favorites"
                }
            };
            SelectedCategory = Categories[0];

            _storage.Categories.ForEach(c => Categories.Add(c));
        }

        public ObservableCollection<StorageEntry> DisplayedEntries
        {
            get => _displayedEntries;
            set
            {
                _displayedEntries = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<StorageCategory> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public StorageCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();

                switch (SelectedCategory.Tag)
                {
                    case "all":
                        DisplayedEntries = new ObservableCollection<StorageEntry>(_storage.Entries);
                        break;
                    case "favorites":
                        DisplayedEntries =
                            new ObservableCollection<StorageEntry>(_storage.Entries.Where(e => e.IsFavorite));
                        break;
                    default:
                        var entries = _storage.Entries.Where(e => e.Category != null)
                            .Where(e => e.Category.Equals(SelectedCategory));

                        DisplayedEntries =
                            new ObservableCollection<StorageEntry>(entries);
                        break;
                }
            }
        }

        public StorageEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged();

                DisplayedPassword = SelectedEntry == null ? null : "•••••";
                OnPropertyChanged(nameof(DisplayedPassword));
            }
        }

        public string DisplayedPassword
        {
            get => _displayedPassword;
            set
            {
                _displayedPassword = value;
                OnPropertyChanged();
            }
        }

        private void ListBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedEntry = null;
        }

        private void StarUnstarSelectedEntry_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedEntry.IsFavorite = !SelectedEntry.IsFavorite;

            var originalEntry = _storage.Entries.Single(s => s.Equals(SelectedEntry));
            originalEntry = SelectedEntry;

            OnPropertyChanged(nameof(SelectedEntry));
            SelectedCategory = SelectedCategory; // Update category items
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
