using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LCrypt.ViewModels;

namespace LCrypt
{
    public sealed class LCryptFunction : NotifyPropertyChanged
    {
        public LCryptFunction(string name, object content)
        {
            Name = name;
            LocalizedName = Name; // TODO
            Content = content;
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

        private object _content;
        public object Content
        {
            get => _content;
            set => SetAndNotify(ref _content, value);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
