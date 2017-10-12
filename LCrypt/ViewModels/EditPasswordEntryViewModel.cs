using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LCrypt.Models;
using LCrypt.Utility;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security;
using System.Windows;
using LCrypt.Views;

namespace LCrypt.ViewModels
{
    public class EditPasswordEntryViewModel : NotifyPropertyChanged
    {
        private string _dialogTitle;

        private PasswordEntry _entry;
        private SecureString _password;

        private List<string> _icons;

        private List<PasswordCategory> _categories;

        public EditPasswordEntryViewModel()
        {
            Icons = new List<string>(277);
            for (var i = 0; i < 278; i++)
            {
                Icons.Add($"pack://application:,,/Resources/PasswordManagerIcons/Entry/{i}.png");
            }
        }

        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetAndNotify(ref _dialogTitle, value);
        }

        public PasswordEntry Entry
        {
            get => _entry;
            set => SetAndNotify(ref _entry, value);
        }

        public SecureString Password
        {
            get => _password;
            set => SetAndNotify(ref _password, value);
        }

        public List<string> Icons
        {
            get => _icons;
            set => SetAndNotify(ref _icons, value);
        }

        public List<PasswordCategory> Categories
        {
            get => _categories;
            set => SetAndNotify(ref _categories, value);
        }

        public ICommand CancelOrSaveCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Debug.Assert(obj == null || obj is PasswordEntry);
                    DialogHost.CloseDialogCommand.Execute(parameter: obj, target: null);
                }, e =>
                {
                    if (e == null) return true;
                    Debug.Assert(e is PasswordEntry);

                    var entry = (PasswordEntry)e;
                    return !string.IsNullOrWhiteSpace(entry.Name) && Password?.Length > 0;
                });
            }
        }

        public ICommand RemoveCategoryCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    Debug.Assert(Entry?.Category != null);
                    Entry.Category = null;
                }, _ => Entry?.Category != null);
            }
        }

        public void Reset()
        {
            Entry = null;
            Password = null;
        }
    }
}
