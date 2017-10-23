using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LCrypt.ViewModels
{
    [DataContract]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (ReferenceEquals(field, value)) return;
            if (propertyName == null) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
