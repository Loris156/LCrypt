using System;
using LCrypt.Views;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using LCrypt.Models;

namespace LCrypt.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private ICollectionView _functionView;

        public MainViewModel()
        {
            var passwordManagerLoginViewModel = new PasswordManagerLoginViewModel();
            passwordManagerLoginViewModel.LoggedInSuccessfully += PasswordManager_OnSuccessfullLogin;

            var passwordManagerFunction = new LCryptFunction("PasswordManager", new PasswordManagerLoginView
            {
                DataContext = passwordManagerLoginViewModel
            })
            {
                PackIconKind = PackIconKind.Key
            };

            IEnumerable<LCryptFunction> functions = new List<LCryptFunction>
            {
                new LCryptFunction("Home", new HomeView
                {
                    DataContext = new HomeViewModel()
                })
                {
                    PackIconKind = PackIconKind.Home
                },
                passwordManagerFunction,
                new LCryptFunction("FileEncryption", new FileEncryptionView
                {
                    DataContext = new FileEncryptionViewModel()
                })
                {
                    PackIconKind = PackIconKind.FileLock
                },
                new LCryptFunction("FileChecksum", new FileChecksumView
                {
                    DataContext = new FileChecksumViewModel()
                })
                {
                    PackIconKind = PackIconKind.FileCheck
                },
                new LCryptFunction("TextHashing", new TextHashingView
                {
                    DataContext = new TextHashingViewModel()
                })
                {
                    PackIconKind = PackIconKind.TextShadow
                },
                new LCryptFunction("Settings", null)
                {
                    PackIconKind = PackIconKind.Settings
                },
                new LCryptFunction("About", null)
                {
                    PackIconKind = PackIconKind.Information
                }
            };


            Functions = CollectionViewSource.GetDefaultView(functions);
            SelectedFunction = DisplayedFunction = (LCryptFunction)Functions.CurrentItem;
        }

        private bool _leftDrawerOpen;
        public bool LeftDrawerOpen
        {
            get => _leftDrawerOpen;
            set => SetAndNotify(ref _leftDrawerOpen, value);
        }

        public ICollectionView Functions
        {
            get => _functionView;
            set => SetAndNotify(ref _functionView, value);
        }

        private LCryptFunction _selectedFunction;
        public LCryptFunction SelectedFunction
        {
            get => _selectedFunction;
            set
            {
                SetAndNotify(ref _selectedFunction, value);
                if (SelectedFunction != null)
                    DisplayedFunction = SelectedFunction;
            }
        }

        private LCryptFunction _displayedFunction;
        public LCryptFunction DisplayedFunction
        {
            get => _displayedFunction;
            set
            {
                SetAndNotify(ref _displayedFunction, value);
                LeftDrawerOpen = false;
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value?.ToLowerInvariant();
                OnPropertyChanged();

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    Functions.Filter = null;
                    return;
                }

                Functions.Filter = func =>
                {
                    var function = (LCryptFunction)func;

                    return function.Name?.ToLowerInvariant().Contains(SearchText) == true ||
                           function.LocalizedName?.ToLowerInvariant().Contains(SearchText) == true;
                };
            }
        }

        private void PasswordManager_OnSuccessfullLogin(object sender, PasswordStorage e)
        {
            var loginViewModel = (PasswordManagerLoginViewModel)sender;
            loginViewModel.LoggedInSuccessfully -= PasswordManager_OnSuccessfullLogin;

            var function = Functions.OfType<LCryptFunction>().Single(f => f.Name == "PasswordManager");

            var passwordManagerViewModel = new PasswordManagerViewModel(e);
            passwordManagerViewModel.Logout += PasswordManager_OnLogout;

            function.Content = new PasswordManagerView
            {
                DataContext = passwordManagerViewModel
            };
        }

        private void PasswordManager_OnLogout(object sender, EventArgs e)
        {
            var passwordManagerViewModel = (PasswordManagerViewModel)sender;
            passwordManagerViewModel.Logout -= PasswordManager_OnLogout;

            var function = Functions.OfType<LCryptFunction>().Single(f => f.Name == "PasswordManager");

            var passwordManagerLoginViewModel = new PasswordManagerLoginViewModel();
            passwordManagerLoginViewModel.LoggedInSuccessfully += PasswordManager_OnSuccessfullLogin;

            function.Content = new PasswordManagerLoginView
            {
                DataContext = passwordManagerLoginViewModel
            };

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
