using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls.Dialogs;
using Localization = LCrypt.Properties.Localization;

namespace LCrypt.Password_Manager
{
    public partial class EditEntryWindow : INotifyPropertyChanged
    {
        private bool _close;
        private string _windowName;

        private StorageEntry _entry;
        private string _password;

        private ObservableCollection<StorageCategory> _categories;
        private ObservableCollection<BitmapImage> _icons;

        public EditEntryWindow(StorageEntry entry, IEnumerable<StorageCategory> categories)
        {
            InitializeComponent();
            DataContext = this;

            if (entry == null)
            {
                Entry = new StorageEntry();
                WindowName = Localization.NewEntry;
            }
            else
            {
                Entry = entry.Clone() as StorageEntry;
                WindowName = Localization.EditEntry + " - " + EntryName;
            }

            // ReSharper disable once PossibleNullReferenceException
            Category = Entry.Category;
            Categories = new ObservableCollection<StorageCategory>(categories.Where(c => string.IsNullOrEmpty(c.Tag)));

            Icons = new ObservableCollection<BitmapImage>();
            for (var i = 0; i < 278; ++i)
            {
                Icons.Add(new BitmapImage(
                    new Uri($"pack://application:,,/Resources/Password Manager Icons/Entry/{i}.png")));
            }

            ShowPasswordCommand = new RelayCommand(async _ =>
            {
                await this.ShowMessageAsync(Localization.Password, Password);
            });

            RemoveCategoryCommand = new RelayCommand(_ =>
            {
                Category = null;
            });

            SaveCommand = new RelayCommand(_ =>
            {
                Save = true;
                _close = true;
                Close();
            },
            _ => !string.IsNullOrWhiteSpace(EntryName) && !string.IsNullOrWhiteSpace(Password));

            CancelCommand = new RelayCommand(async _ =>
            {
                if (await this.ShowMessageAsync(Localization.PasswordManager, Localization.CancelEditing,
                        MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                        {
                            AffirmativeButtonText = Localization.No,
                            NegativeButtonText = Localization.Yes,
                            AnimateHide = false
                        }) != MessageDialogResult.Negative) return;
                _close = true;
                Close();
            });
        }

        public string WindowName
        {
            get => _windowName;
            set
            {
                _windowName = value;
                OnPropertyChanged();
            }
        }

        public StorageEntry Entry
        {
            get => _entry;
            set
            {
                _entry = value;
                OnPropertyChanged();
            }
        }

        public string EntryName
        {
            get => Entry.Name;
            set
            {
                Entry.Name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowName));
            }
        }

        public int IconId
        {
            get => Entry.IconId;
            set
            {
                Entry.IconId = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => Entry.Username;
            set
            {
                Entry.Username = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => Entry.Email;
            set
            {
                Entry.Email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get => Entry.Comment;
            set
            {
                Entry.Comment = value;
                OnPropertyChanged();
            }
        }

        public StorageCategory Category
        {
            get => Entry.Category;
            set
            {
                Entry.Category = value;
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

        public ObservableCollection<BitmapImage> Icons
        {
            get => _icons;
            set
            {
                _icons = value;
                OnPropertyChanged();
            }
        }

        public bool Save { get; set; }

        public ICommand ShowPasswordCommand { get; }
        public ICommand RemoveCategoryCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = (sender as PasswordBox).Password;
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (_close) return;
            e.Cancel = true;
            CancelCommand.Execute(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
