using LCrypt.Enumerations;
using LCrypt.Utility;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private bool _isBusy;

        private string _searchQuery;
        private SearchScope _searchScope;

        private bool _isSettingsOpen;

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

            SearchScope = SearchScope.Everything;

            OpenSettingsCommand = new RelayCommand(_ =>
            {
                IsSettingsOpen = !IsSettingsOpen;
            }, new KeyGesture(Key.P, ModifierKeys.Control));

            QuitManagerCommand = new RelayCommand(_ =>
            {
                this.Close();
            }, new KeyGesture(Key.Q, ModifierKeys.Control));

            AddEntryCommand = new RelayCommand(async _ =>
            {
                var addEntryWindow = new EditEntryWindow(null, Categories);
                addEntryWindow.ShowDialog();

                if (!addEntryWindow.Save) return;
                addEntryWindow.Entry.Password = await _storage.Aes.EncryptStringAsync(addEntryWindow.Password);
                _storage.Entries.Add(addEntryWindow.Entry);
                SelectedCategory = SelectedCategory;

                try
                {
                    await _storage.SaveAsync();
                }
                catch (Exception)
                {
                    await this.ShowMessageAsync(Localization.PasswordManager, Localization.CouldNotSaveStorage);
                }
            }, new KeyGesture(Key.N, ModifierKeys.Control));

            EditEntryCommand = new RelayCommand(async _ =>
                {
                    var editEntryWindow = new EditEntryWindow(SelectedEntry, Categories)
                    {
                        PasswordBox = { Password = await _storage.Aes.DecryptStringAsync(SelectedEntry.Password) }
                    };
                    editEntryWindow.ShowDialog();

                    if (!editEntryWindow.Save) return;

                    var newEntry = editEntryWindow.Entry;
                    newEntry.Password = await _storage.Aes.EncryptStringAsync(editEntryWindow.Password);
    
                    var index = _storage.Entries.FindIndex(e => e.Equals(newEntry));
                    if (index == -1) return;
                    _storage.Entries[index] = newEntry;

                    SelectedCategory = SelectedCategory;
                    SelectedEntry = newEntry;

                    try
                    {
                        await _storage.SaveAsync();
                    }
                    catch (Exception)
                    {
                        await this.ShowMessageAsync(Localization.PasswordManager, Localization.CouldNotSaveStorage);
                    }
                },
                _ => SelectedEntry != null,
                new KeyGesture(Key.E, ModifierKeys.Control));

            DeleteEntryCommand = new RelayCommand(async _ =>
                {
                    if (await this.ShowMessageAsync(Localization.PasswordManager,
                            string.Format(Localization.ReallyDeleteEntry, SelectedEntry.Name),
                            MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                            {
                                AffirmativeButtonText = Localization.Yes,
                                NegativeButtonText = Localization.No
                            }) != MessageDialogResult.Affirmative) return;
                    _storage.Entries.Remove(SelectedEntry);
                    SelectedCategory = SelectedCategory;
                    OnPropertyChanged(nameof(SelectedEntry));
                    try
                    {
                        await _storage.SaveAsync();
                    }
                    catch (Exception)
                    {
                        await this.ShowMessageAsync(Localization.PasswordManager, Localization.CouldNotSaveStorage);
                    }
                },
                _ => SelectedEntry != null,
                new KeyGesture(Key.Delete));

            EditStorageNameCommand = new RelayCommand(async _ =>
            {
                var newName = await this.ShowInputAsync(Localization.PasswordManager,
                    Localization.PasswordManagerNewName,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = Localization.Save,
                        NegativeButtonText = Localization.Cancel,
                        DefaultText = _storage.Name
                    });
                if (string.IsNullOrWhiteSpace(newName)) return;

                _storage.Name = newName;
                Categories[0].Name = _storage.Name;
                Categories = new ObservableCollection<StorageCategory>(Categories);

                try
                {
                    await _storage.SaveAsync();
                }
                catch (Exception)
                {
                    await this.ShowMessageAsync(Localization.PasswordManager, Localization.CouldNotSaveStorage);
                }
            });

            EditMasterPasswordCommand = new RelayCommand(async _ =>
            {
                SelectedEntry = null;

                var oldPassword = await this.ShowLoginAsync(Localization.MasterPassword,
                    Localization.TypeInOldMasterPassword, new LoginDialogSettings
                    {
                        ShouldHideUsername = true,
                        PasswordWatermark = Localization.Password,
                        AffirmativeButtonText = Localization.Continue,
                        NegativeButtonVisibility = Visibility.Visible,
                        NegativeButtonText = Localization.Cancel
                    });
                if (string.IsNullOrWhiteSpace(oldPassword?.Password)) return;

                if (!_storage.ValidatePassword(oldPassword.Password))
                {
                    await this.ShowMessageAsync(Localization.MasterPassword, Localization.InvalidPassword,
                        MessageDialogStyle.Affirmative, new MetroDialogSettings
                        {
                            AffirmativeButtonText = Localization.Cancel
                        });
                    return;
                }

                var passwordsAreEqual = false;
                string password;
                do
                {
                    var firstPasswordInput = await this.ShowLoginAsync(Localization.MasterPassword,
                        Localization.TypeInNewMasterPassword, new LoginDialogSettings
                        {
                            ShouldHideUsername = true,
                            PasswordWatermark = Localization.Password,
                            AffirmativeButtonText = Localization.Continue,
                            NegativeButtonVisibility = Visibility.Visible,
                            NegativeButtonText = Localization.Cancel
                        });
                    if (string.IsNullOrWhiteSpace(firstPasswordInput?.Password)) return;

                    var secondPasswordInput = await this.ShowLoginAsync(Localization.MasterPassword,
                        Localization.TypeInNewMasterPasswordAgain, new LoginDialogSettings
                        {
                            ShouldHideUsername = true,
                            PasswordWatermark = Localization.Password,
                            AffirmativeButtonText = Localization.Continue,
                            NegativeButtonVisibility = Visibility.Visible,
                            NegativeButtonText = Localization.Cancel
                        });
                    if (string.IsNullOrWhiteSpace(secondPasswordInput?.Password)) return;

                    if (firstPasswordInput.Password.Equals(secondPasswordInput.Password))
                        passwordsAreEqual = true;
                    else if (await this.ShowMessageAsync(Localization.MasterPassword, Localization.PasswordsAreNotEqual,
                                 MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                                 {
                                     AffirmativeButtonText = Localization.Retry,
                                     NegativeButtonText = Localization.Cancel
                                 }) == MessageDialogResult.Negative)
                        return;
                    password = firstPasswordInput.Password;

                } while (!passwordsAreEqual);

                var controller = await this.ShowProgressAsync(Localization.PleaseWait, Localization.ChangingMasterPassword);
                controller.SetProgress(0);
                controller.Maximum = _storage.Entries.Count;

                var progress = new Progress<int>(x => controller.SetProgress(x));

                await _storage.ChangeMasterPassword(password, progress);
                try
                {
                    await _storage.SaveAsync();
                }
                catch (Exception)
                {
                    await this.ShowMessageAsync(Localization.PasswordManager, Localization.CouldNotSaveStorage);
                }

                await controller.CloseAsync();
            });

            DuplicateEntryCommand = new RelayCommand(async _ =>
                {
                    _storage.Entries.Add(SelectedEntry.Duplicate());
                    try
                    {
                        await _storage.SaveAsync();
                    }
                    catch (Exception)
                    {
                        await this.ShowMessageAsync(Localization.PasswordManager, Localization.CouldNotSaveStorage);
                    }
                    SelectedCategory = SelectedCategory;
                },
            _ => SelectedEntry != null,
            new KeyGesture(Key.D, ModifierKeys.Control));

            OpenWikiCommand = new RelayCommand(_ =>
            {
                Process.Start(PasswordManagerWikiUrl);

            }, new KeyGesture(Key.F1));
        }

        public static string PasswordManagerWikiUrl { get; } = "https://www.github.com/Loris156/LCrypt/wiki/Password-Manager/";

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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        public SearchScope SearchScope
        {
            get => _searchScope;
            set
            {
                _searchScope = value;
                OnPropertyChanged();
            }
        }

        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set
            {
                _isSettingsOpen = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenSettingsCommand { get; }
        public ICommand QuitManagerCommand { get; }

        public ICommand AddEntryCommand { get; }
        public ICommand EditEntryCommand { get; }
        public ICommand DeleteEntryCommand { get; }
        public ICommand DuplicateEntryCommand { get; }

        public ICommand EditStorageNameCommand { get; }
        public ICommand EditMasterPasswordCommand { get; }

        public ICommand OpenWikiCommand { get; }

        private void ListBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedEntry = null;
        }

        private async void ListBoxCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var entryGuid = (Guid)((Button)sender).Tag;
                var entry = DisplayedEntries.Single(x => x.Guid.Equals(entryGuid));
                Util.CopyFor(await _storage.Aes.DecryptStringAsync(entry.Password), TimeSpan.FromSeconds(10));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async void StarUnstarSelectedEntry_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedEntry.IsFavorite = !SelectedEntry.IsFavorite;

            var index = _storage.Entries.FindIndex(x => x.Equals(SelectedEntry));
            if (index == -1) return;
            _storage.Entries[index] = SelectedEntry;

            OnPropertyChanged(nameof(SelectedEntry));
            SelectedCategory = SelectedCategory; // Update category items

            try
            {
                await _storage.SaveAsync();
            }
            catch (Exception)
            {
                await this.ShowMessageAsync(Localization.PasswordManager, Localization.CouldNotSaveStorage);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void DisplayPassword_OnClick(object sender, RoutedEventArgs e)
        {
            if (DisplayedPassword != "•••••")
                DisplayedPassword = "•••••";
            else
                DisplayedPassword = await _storage.Aes.DecryptStringAsync(SelectedEntry?.Password);
            OnPropertyChanged(nameof(DisplayedPassword));
        }

        private async void SelectedEntryCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var tag = (string)((Button)sender).Tag;
                string toCopy;

                switch (tag)
                {
                    case "Username":
                        toCopy = SelectedEntry.Username;
                        break;
                    case "Email":
                        toCopy = SelectedEntry.Email;
                        break;
                    case "Password":
                        Util.CopyFor(await _storage.Aes.DecryptStringAsync(SelectedEntry.Password),
                            TimeSpan.FromSeconds(10));
                        return;
                    case "Comment":
                        toCopy = SelectedEntry.Comment;
                        break;
                    default: return;
                }

                Clipboard.SetText(toCopy);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void SearchEntries(object sender, RoutedEventArgs e)
        {
            SelectedCategory = Categories[0];

            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                DisplayedEntries = new ObservableCollection<StorageEntry>(_storage.Entries);
                return;
            }

            var search = SearchQuery.ToUpper();
            IEnumerable<StorageEntry> entries;

            switch (SearchScope)
            {
                case SearchScope.Everything:
                    entries = _storage.Entries.Where(x => x.Name.ToUpper().Contains(search) ||
                                                          x.Username.ToUpper().Contains(search) ||
                                                          x.Email.ToUpper().Contains(search) ||
                                                          x.Comment.ToUpper().Contains(search));
                    break;
                case SearchScope.Name:
                    entries = _storage.Entries.Where(x => x.Name.ToUpper().Contains(search));
                    break;
                case SearchScope.Username:
                    entries = _storage.Entries.Where(x => x.Username.ToUpper().Contains(search));
                    break;
                case SearchScope.Email:
                    entries = _storage.Entries.Where(x => x.Email.ToUpper().Contains(search));
                    break;
                case SearchScope.Comment:
                    entries = _storage.Entries.Where(x => x.Comment.ToUpper().Contains(search));
                    break;
                default:
                    entries = Enumerable.Empty<StorageEntry>();
                    break;
            }

            DisplayedEntries = new ObservableCollection<StorageEntry>(entries);
        }
    }
}
