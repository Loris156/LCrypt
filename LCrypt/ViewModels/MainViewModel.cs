using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using LCrypt.Utility;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;

namespace LCrypt.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private List<LCryptFunction> _functions;
        private ICollectionView _functionView;

        public MainViewModel()
        {
            _functions = new List<LCryptFunction>()
            {
                new LCryptFunction("Home", null),
                new LCryptFunction("Settings", null)
            };

            _functionView = CollectionViewSource.GetDefaultView(_functions);
            _functionView.CurrentChanged += CloseLeftDrawer;
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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value.ToLowerInvariant();
                OnPropertyChanged();
                ApplySearchFilter();
            }
        }

        private void ApplySearchFilter()
        {
            _functionView.CurrentChanged -= CloseLeftDrawer;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Functions.Filter = null;
                _functionView.CurrentChanged += CloseLeftDrawer;
                return;
            }

            Functions.Filter = (func) =>
            {
                var function = (LCryptFunction)func;
                return function.Name.ToLowerInvariant().Contains(SearchText) ||
                       function.LocalizedName.ToLowerInvariant().Contains(SearchText);
            };
            _functionView.CurrentChanged += CloseLeftDrawer;
        }

        private void CloseLeftDrawer(object sender, EventArgs args)
        {
            LeftDrawerOpen = false;
        }
    }
}
