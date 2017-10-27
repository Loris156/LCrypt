﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LCrypt.Utility;
using LCrypt.ViewModels;
using MaterialDesignThemes.Wpf;

namespace LCrypt
{
    public sealed class LCryptFunction : NotifyPropertyChanged
    {
        public LCryptFunction(string name, FrameworkElement content)
        {
            Name = name;
            LocalizedName = (string)App.LocalizationDictionary[name];
            Content = content;
            _packIcon = new PackIcon
            {
                Kind = PackIconKind.Account
            };
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        private string _localizedName;
        public string LocalizedName
        {
            get => _localizedName;
            set => SetAndNotify(ref _localizedName, value);
        }

        private readonly PackIcon _packIcon;
        public PackIconKind PackIconKind
        {
            get => _packIcon.Kind;
            set
            {
                _packIcon.Kind = value;
                OnPropertyChanged();
            }
        }

        private FrameworkElement _content;
        public FrameworkElement Content
        {
            get => _content;
            set => SetAndNotify(ref _content, value);
        }

        public ViewModelBase ViewModel
        {
            get => (ViewModelBase) Content?.DataContext;
            set
            {
                Content.DataContext = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return LocalizedName;
        }
    }
}
