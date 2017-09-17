using LCrypt.Views;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace LCrypt.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private ICollectionView _functionView;

        public MainViewModel()
        {
            IEnumerable<LCryptFunction> functions = new List<LCryptFunction>()
            {
                new LCryptFunction("Home", new HomeView
                {
                    DataContext = new HomeViewModel()
                })
                {
                    PackIconKind = PackIconKind.Home
                },
                new LCryptFunction("FileEncryption", new FileEncryptionView
                {
                    DataContext = new FileEncryptionViewModel()
                })
                {
                    PackIconKind = PackIconKind.File
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
    }
}
