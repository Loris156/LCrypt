using LCrypt.Models;
using LCrypt.Utility;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;
using LCrypt.Utility.Extensions;
using LCrypt.Views;

namespace LCrypt.ViewModels
{
    public class PasswordManagerViewModel : NotifyPropertyChanged
    {
        public static string WalletFileName => "wallet.lcrypt";

        public static int SaltLength => 32;

        public static int Pbkdf2Iterations => 50000;

        public const int FileBufferSize = 131072; // 128 KiB

        private readonly PasswordStorage _passwordStorage;
        private bool _dialogOpen;

        private ListCollectionView _entries;
        private ObservableCollection<PasswordEntry> _selectedEntries;
        private PasswordEntry _selectedEntry;

        private string _displayedPassword;

        private SnackbarMessageQueue _snackbarMessageQueue;

        // No good MVVM approach, please change if you know a better method
        private EditPasswordEntryView _editPasswordEntryView;
        private EditPasswordEntryViewModel _editPasswordEntryViewModel;

        public PasswordManagerViewModel(PasswordStorage passwordStorage)
        {
            _passwordStorage = passwordStorage;

            Entries = new ListCollectionView(_passwordStorage.Entries);
            SelectedEntries = new ObservableCollection<PasswordEntry>();
            SelectedEntries.CollectionChanged += SelectedEntries_OnCollectionChanged;

            SnackbarMessageQueue = new SnackbarMessageQueue();
        }

        public ListCollectionView Entries
        {
            get => _entries;
            set => SetAndNotify(ref _entries, value);
        }

        public ObservableCollection<PasswordEntry> SelectedEntries
        {
            get => _selectedEntries;
            set => SetAndNotify(ref _selectedEntries, value);
        }

        public PasswordEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                SetAndNotify(ref _selectedEntry, value);
                if (SelectedEntry != null)
                    DisplayedPassword = "•••••";
            }
        }

        public string DisplayedPassword
        {
            get => _displayedPassword;
            set => SetAndNotify(ref _displayedPassword, value);
        }

        public SnackbarMessageQueue SnackbarMessageQueue
        {
            get => _snackbarMessageQueue;
            set => SetAndNotify(ref _snackbarMessageQueue, value);
        }

        public bool ShowToolbar
        {
            get => _passwordStorage.ShowToolbar;
            set
            {
                _passwordStorage.ShowToolbar = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler Logout;

        public ICommand LogoutCommand
        {
            get
            {
                return new RelayCommand(async _ =>
                {
                    _dialogOpen = true;
                    try
                    {
                        if (await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["PasswordManager"],
                                (string)App.LocalizationDictionary["ReallyLogout"],
                                MessageDialogStyle.AffirmativeAndNegative,
                                new MetroDialogSettings
                                {
                                    AffirmativeButtonText = (string)App.LocalizationDictionary["Yes"],
                                    NegativeButtonText = (string)App.LocalizationDictionary["No"],
                                    CustomResourceDictionary = App.DialogDictionary,
                                    SuppressDefaultResources = true
                                }) != MessageDialogResult.Affirmative) return;

                        await SaveStorageAsync();
                        _passwordStorage.Dispose();
                        Logout?.Invoke(this, EventArgs.Empty);
                    }
                    finally
                    {
                        _dialogOpen = false;
                    }
                }, _ => !_dialogOpen);
            }
        }

        public ICommand AddEntryCommand
        {
            get
            {
                return new RelayCommand(async view =>
                {
                    if (_editPasswordEntryViewModel == null)
                    {
                        _editPasswordEntryViewModel = new EditPasswordEntryViewModel();
                        _editPasswordEntryView = new EditPasswordEntryView
                        {
                            DataContext = _editPasswordEntryViewModel
                        };
                    }

                    _editPasswordEntryViewModel.DialogTitle = (string)App.LocalizationDictionary["NewEntry"];
                    _editPasswordEntryViewModel.Entry = new PasswordEntry();
                    _editPasswordEntryViewModel.Categories = _passwordStorage.Categories;

                    var newItem = await DialogHost.Show(_editPasswordEntryView);
                    if (newItem == null)
                    {
                        _editPasswordEntryViewModel.Reset();
                        return;
                    }

                    Debug.Assert(newItem is PasswordEntry);
                    var newEntry = (PasswordEntry)newItem;
                    newEntry.Password =
                        await _passwordStorage.Aes.EncryptStringAsync(_editPasswordEntryViewModel.SecurePassword
                            .ToInsecureString());
                    Entries.AddNewItem(newEntry);
                    Entries.CommitNew();
                    await SaveStorageAsync();

                    // Sets password to null and removes selected entry
                    _editPasswordEntryViewModel.Reset();
                });
            }
        }

        public ICommand EditEntryCommand
        {
            get
            {
                return new RelayCommand(async _ =>
                {
                    if (_editPasswordEntryViewModel == null)
                    {
                        _editPasswordEntryViewModel = new EditPasswordEntryViewModel();
                        _editPasswordEntryView = new EditPasswordEntryView
                        {
                            DataContext = _editPasswordEntryViewModel
                        };
                    }

                    var editEntry = SelectedEntries[0];
                    Entries.EditItem(editEntry);
                    DisplayedPassword = "•••••";

                    _editPasswordEntryViewModel.DialogTitle = (string)App.LocalizationDictionary["EditEntry"];
                    _editPasswordEntryViewModel.Entry = editEntry;
                    _editPasswordEntryViewModel.Categories = _passwordStorage.Categories;

                    _editPasswordEntryViewModel.Password =
                        await _passwordStorage.Aes.DecryptStringAsync(editEntry.Password);

                    var editResult = await DialogHost.Show(_editPasswordEntryView);
                    if (editResult == null)
                    {
                        Entries.CancelEdit();
                        _editPasswordEntryViewModel.Reset();
                        return;
                    }

                    Debug.Assert(editResult != null);
                    Debug.Assert(editResult is PasswordEntry);
                    editEntry.Password =
                        await _passwordStorage.Aes.EncryptStringAsync(_editPasswordEntryViewModel.SecurePassword
                            .ToInsecureString());

                    editEntry.LastModified = DateTime.Now;
                    Entries.CommitEdit();
                    await SaveStorageAsync();

                    _editPasswordEntryViewModel.Reset();

                }, _ => SelectedEntries.Count == 1);
            }
        }

        public ICommand DeleteEntriesCommand
        {
            get
            {
                return new RelayCommand(async _ =>
                {
                    if (SelectedEntries.Count == 1)
                    {
                        if (await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["Warning"],
                                string.Format((string)App.LocalizationDictionary["ReallyDeleteEntry"],
                                    SelectedEntries[0]),
                                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                                {
                                    AffirmativeButtonText = (string)App.LocalizationDictionary["Yes"],
                                    NegativeButtonText = (string)App.LocalizationDictionary["No"],
                                    CustomResourceDictionary = App.DialogDictionary,
                                    SuppressDefaultResources = true
                                }) == MessageDialogResult.Affirmative)
                            Entries.Remove(SelectedEntries[0]);
                    }
                    else
                    {
                        if (await DialogCoordinator.Instance.ShowMessageAsync(this,
                                (string)App.LocalizationDictionary["Warning"],
                                string.Format((string)App.LocalizationDictionary["ReallyDeleteEntries"],
                                    SelectedEntries.Count),
                                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                                {
                                    AffirmativeButtonText = (string)App.LocalizationDictionary["Yes"],
                                    NegativeButtonText = (string)App.LocalizationDictionary["No"],
                                    CustomResourceDictionary = App.DialogDictionary,
                                    SuppressDefaultResources = true
                                }) == MessageDialogResult.Affirmative)
                            while (SelectedEntries.Count > 0)
                                Entries.Remove(SelectedEntries[0]);
                    }
                    await SaveStorageAsync();

                }, _ => SelectedEntries.Count > 0);
            }
        }

        public ICommand DuplicateEntryCommand
        {
            get
            {
                return new RelayCommand(async _ =>
                {
                    await SaveStorageAsync();
                    SnackbarMessageQueue.Enqueue("Storage saved!");
                });
            }
        }

        public ICommand ShowPasswordCommand
        {
            get
            {
                return new RelayCommand(async _ =>
                {
                    if (DisplayedPassword != "•••••")
                        DisplayedPassword = "•••••";
                    else
                        DisplayedPassword = await _passwordStorage.Aes.DecryptStringAsync(SelectedEntry.Password);
                }, _ => SelectedEntry != null);
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                return new RelayCommand(async target =>
                {
                    Debug.Assert(target != null);
                    string copyValue;

                    switch (target)
                    {
                        case string _:
                            switch (target)
                            {
                                case "Username":
                                    copyValue = SelectedEntry.Username;
                                    break;
                                case "Email":
                                    copyValue = SelectedEntry.Email;
                                    break;
                                case "Password":
                                    copyValue = await _passwordStorage.Aes.DecryptStringAsync(SelectedEntry.Password);
                                    break;
                                case "Url":
                                    copyValue = SelectedEntry.Url;
                                    break;
                                case "Comment":
                                    copyValue = SelectedEntry.Comment;
                                    break;
                                default:
                                    throw new ArgumentException("Name does not match any property.", nameof(target));
                            }
                            break;
                        case PasswordEntry e:
                            copyValue = await _passwordStorage.Aes.DecryptStringAsync(e.Password);
                            break;
                        default:
                            throw new ArgumentException("Target was neither a string nor a PasswordEntry.", nameof(target));
                    }

                    Debug.Assert(copyValue != null);
                    Clipboard.SetText(copyValue, TextDataFormat.UnicodeText);

                    SnackbarMessageQueue.Enqueue(
                        target is PasswordEntry
                            ? $"{(string)App.LocalizationDictionary["Password"]} {(string)App.LocalizationDictionary["Copied"]}!"
                            : $"{(string)App.LocalizationDictionary[target]} {(string)App.LocalizationDictionary["Copied"]}!",
                        (string)App.LocalizationDictionary["Undo"],
                        Clipboard.Clear);
                },
                target =>
                {
                    if (target is PasswordEntry) return true; // ListBox button should always work.
                    if (SelectedEntry == null) return false;

                    Debug.Assert(target != null);
                    Debug.Assert(target is string);

                    string copyValue;

                    switch (target)
                    {
                        case "Username":
                            copyValue = SelectedEntry.Username;
                            break;
                        case "Email":
                            copyValue = SelectedEntry.Email;
                            break;
                        case "Password":
                            return true;
                        case "Url":
                            copyValue = SelectedEntry.Url;
                            break;
                        case "Comment":
                            copyValue = SelectedEntry.Comment;
                            break;
                        default:
                            throw new ArgumentException("Name does not match any property.", nameof(target));
                    }

                    return !string.IsNullOrWhiteSpace(copyValue);
                });
            }
        }

        public ICommand OpenUrlCommand
        {
            get
            {
                return new RelayCommand(e =>
                {
                    Debug.Assert(e == null || e is string);

                    try
                    {
                        if ((string)e == "Email")
                            Process.Start("mailto:" + SelectedEntry.Email);
                        else
                            Process.Start(SelectedEntry.Url);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Process.Start("www." + SelectedEntry.Url);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }, e =>
                {
                    if (e is string s && s == "Email")
                        return !string.IsNullOrWhiteSpace(SelectedEntry?.Email);
                    return !string.IsNullOrWhiteSpace(SelectedEntry?.Url);
                });
            }
        }

        private void SelectedEntries_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedEntry = SelectedEntries.Count == 1 ? SelectedEntries[0] : null;
        }

        private async Task<string> SaveStorageAsync()
        {
            try
            {
                using (var fs =
                    new FileStream(
                        Path.Combine(App.MyDocuments, "LCrypt", WalletFileName),
                        FileMode.Create, FileAccess.Write, FileShare.None,
                        FileBufferSize, useAsync: true))
                {
                    await fs.WriteAsync(_passwordStorage.Salt, 0, SaltLength);
                    await fs.WriteAsync(_passwordStorage.Aes.IV, 0, _passwordStorage.Aes.BlockSize / 8);

                    using (var transform = _passwordStorage.Aes.CreateEncryptor())
                    {
                        using (var cryptoStream = new CryptoStream(fs, transform, CryptoStreamMode.Write))
                        {
                            using (var xmlWriter = XmlDictionaryWriter.CreateBinaryWriter(cryptoStream))
                            {
                                var serializer = new DataContractSerializer(typeof(PasswordStorage));
                                _passwordStorage.LastModified = DateTime.Now;
                                serializer.WriteObject(xmlWriter, _passwordStorage);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return null;
        }
    }
}