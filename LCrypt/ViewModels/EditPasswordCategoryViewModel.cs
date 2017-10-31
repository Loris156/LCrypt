using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LCrypt.Models;
using LCrypt.Utility;
using MaterialDesignThemes.Wpf;

namespace LCrypt.ViewModels
{
    /// <summary>
    /// ViewModel for password category editing.
    /// </summary>
    public class EditPasswordCategoryViewModel : ViewModelBase
    {
        private string _dialogTitle;

        private PasswordCategory _passwordCategory;

        private List<string> _icons;

        public EditPasswordCategoryViewModel()
        {
            Icons = new List<string>(80);
            for (var i = 0; i < 80; i++)
            {
                Icons.Add($"pack://application:,,/Resources/PasswordManagerIcons/Category/{i}.png");
            }
        }

        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetAndNotify(ref _dialogTitle, value);
        }

        public PasswordCategory PasswordCategory
        {
            get => _passwordCategory;
            set => SetAndNotify(ref _passwordCategory, value);
        }

        public List<string> Icons
        {
            get => _icons;
            set => SetAndNotify(ref _icons, value);
        }

        public ICommand CancelOrSaveCommand
        {
            get
            {
                return new RelayCommand(c =>
                {
                    Debug.Assert(c == null || c is PasswordCategory);
                    DialogHost.CloseDialogCommand.Execute(parameter: c, target: null);
                },
                c =>
                {
                    if (c == null) return true;
                    Debug.Assert(c is PasswordCategory);

                    var category = (PasswordCategory)c;
                    return !string.IsNullOrWhiteSpace(category.Name);
                });
            }
        }

        public void Reset()
        {
            PasswordCategory = null;
        }
    }
}
