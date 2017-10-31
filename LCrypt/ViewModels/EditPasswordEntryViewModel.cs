using LCrypt.Models;
using LCrypt.Utility;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Windows.Input;

namespace LCrypt.ViewModels
{
    public class EditPasswordEntryViewModel : ViewModelBase
    {
        private string _dialogTitle;

        private PasswordEntry _entry;
        private string _password;
        private SecureString _securePassword;

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

        public string Password // The only use of this is to set initial password when editing an entry
        {
            get => _password;
            set => SetAndNotify(ref _password, value);
        }

        public SecureString SecurePassword
        {
            get => _securePassword;
            set => SetAndNotify(ref _securePassword, value);
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
                    return !string.IsNullOrWhiteSpace(entry.Name) && SecurePassword?.Length > 0;
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
            SecurePassword = null;
            Categories = null;
        }
    }
}
